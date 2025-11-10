import React, { useEffect, useState } from "react";
import { showAlert } from "../utils/show-alert";
import axios from "axios";

export default function Sale () {
    const API = "http://localhost:5081/api"
    const [cart, setCart] = useState([])
    const [code, setCode] = useState([])
    const [newQty, setNewQty] = useState({})
    const [total, setTotal] = useState(0)

    const handleCodeInput = (e) => {
        const { value } = e.target
        value == "" ? setCode("") : setCode(Number(value))
    }

    const handleQtyInput = (e, code) => {
        const { value } = e.target
        value == "" ? setNewQty({code: code, qty: ""}) : setNewQty({code: code, qty:Number(value)}) 
    }

    const updateQty = (code, newVal) => {
        let newCart = []
        cart.map(p => {
            if (p.code == code) {
                p["quantity"] = newVal
            }
            newCart.push(p)
        })
        setCart(newCart)
    }

    const changeQty = async (code, qty = null) => {
        const newVal = qty ?? newQty.qty
        if (!newQty?.code || newQty.code != code) return;

        const productInCart = cart.find(p => p.code == code)
        if (productInCart["units"] == "шт" && !Number.isInteger(newVal)) {
            showAlert("Значення не може бути дробовим для даного товару!", "danger")
            setNewQty({})
            return;
        } 

        if (newVal <= 0) {
            removeProduct(code)
            setNewQty({})
            return;
        }

        await axios.post(`${API}/sale/change-qty`, {code, quantity: newVal}, {withCredentials: true})
        .then((r) => {
            updateQty(newQty.code, newVal)
            setNewQty({})
        })
        .catch((e) => {
            if (e.response?.status == 400) {
                const { maxQuantity, message } = e.response.data
                setNewQty({code: code, qty: maxQuantity})
                updateQty(newQty.code, maxQuantity)
                showAlert(message, "danger")
            }
        })
    }

    const addProduct = async () =>  {
        if (code.length < 0) {
            showAlert("Введіть код, щоб додати товар", "danger");
        }
        
        if (code < 0) {
            showAlert("Код введено некоректно", "danger");
        }

        let productInCart = cart.find(p => p.code == code)
        let quantity = productInCart ? productInCart.quantity : 0

        const data = {
            "code": code,
            "quantity": quantity
        }

        await axios.post(`${API}/sale/add-by-code`, data, {withCredentials: true})
        .then((r) => {
            let productToAdd = r.data.product
            if (productInCart) {
                let updatedQty
                const udpatedCart = cart.map((prod, i) => {
                    if (prod.code == code) {
                        updatedQty = prod.quantity + 1
                        prod.quantity = updatedQty
                        return prod;
                    } else {
                        return prod
                    }
                })
                setCart(udpatedCart)

            } else {
                
                const { maxQty } = r.data
                maxQty >= 1 ? productToAdd["quantity"] = 1 : productToAdd["quantity"] = maxQty
                setCart([productToAdd, ...cart])
            }
        })
        .catch((e) => {
            if (e.response?.status == 404 || e.response?.status == 400) {
                showAlert(e.response?.data?.message , "danger");
            } else {
                showAlert("Сталась помилка", "danger");
                console.error(e)
            }
        })
    }

    const removeProduct = (code) => {
        let newCart = []
        cart.map(p => {
            if (p.code != code) newCart.push(p)
        })
        setCart(newCart)
        if (newQty?.code == code) setNewQty({})
    }

    const pay = async (method) => {
        let products = cart.map(p => ({
            code: p.code,
            quantity: p.quantity
        }))

        await axios.post(`${API}/sale/pay`, {method, products}, {withCredentials: true})
        .then((r) => {
            const { message } = r.data
            showAlert(message, "success")
            clearCart()
        })
        .catch((e) => {
            if (e.response?.status == 404 || e.response?.status == 400) {
                showAlert(e.response?.data.message, "danger")
            } else {
                showAlert("Помилка серверу:", "danger")
                console.error(e)
            }
        })
    }

    const clearCart = () => {
        setCart([])
    }

    const calcTotalPrice = () => {
        let total = 0;
        cart.map(p => {
            total += Math.round(p.quantity * p.price * 100) / 100
        })
        return total
    }

    useEffect(() => {
        axios.get(`${API}/home/me`, {withCredentials: true})
        .then((r) => {
            document.title = "Продаж"
        })
        .catch((e) => {
            if (e.response?.status == 401) {
                showAlert("Потрібно увійти", "warning");
                window.location.href = "/login";
            }
        })
    }, [])

    useEffect(() => {
        let total = calcTotalPrice()
        setTotal(total) 
    }, [cart])

    return (
        <>
        <h2 className="text-center">Пункт продажу</h2>

        <div className="row my-3">
            <div className="d-flex align-items-center col-auto">
                <p className="m-0">Код:</p>
            </div>
            <div className="col-auto">
                <input type="number" className="form-control" name="codeAdd" onChange={handleCodeInput} />
            </div>
            <div className="col-auto">
                <button type="button" className="btn btn-success" onClick={addProduct}>+</button>
            </div>
        </div>

        <table className="table table-bordered table-striped align-middle">
            <thead>
                <tr className="fw-bold">
                    <td style={{width: '10%'}}>Код</td>
                    <td>Назва</td>
                    <td style={{width: '15%'}}>Кількість</td>
                    <td style={{width: '15%'}}>Ціна за одиницю</td>
                    <td style={{width: '15%'}}>Одиниці вимірювання</td>
                    <td style={{width: '15%'}}>Сума</td>
                    <td style={{width: '6%'}}></td>
                </tr>
            </thead>
            <tbody>
                {cart.length > 0
                ? cart.map(p => {
                    return (
                    <tr key={p.code}>
                        <td>{p.code}</td>
                        <td>{p.name}</td>
                        <td>
                            <input 
                                name="qty" 
                                type="number" 
                                className="form-control" 
                                value={p.code == newQty?.code ? newQty.qty : p.quantity} 
                                onChange={(e) => handleQtyInput(e, p.code)} 
                                onBlur={(e) => changeQty(p.code)}/>
                        </td>
                        <td>{p.price} ₴</td>
                        <td>{p.units}</td>
                        <td>{Math.round(p.quantity * p.price * 100) / 100} ₴</td>
                        <td>
                            <button className="btn btn-sm btn-outline-danger" onClick={() => removeProduct(p.code)}>❌</button>
                        </td>
                    </tr>
                    )
                    
                })
                : <tr>
                    <td colSpan={7}>
                        <p className="text-muted my-2">Ви не додали ще жодного товару до кошику</p>
                    </td>
                </tr>
                
                }
                <tr className="fw-bold">
                    <td colSpan={5}>Загальна сума:</td>
                    <td >{total} ₴</td>
                    <td><button className="btn btn-sm btn-outline-danger" onClick={() => clearCart()}>🗑️</button></td>
                </tr>
            </tbody>
        </table>

        <div className="d-flex gap-2">
            <button className="btn btn-success" disabled={cart.length < 1} onClick={() => pay("cash")}>Оплата готівкою 💵</button>
            <button className="btn btn-primary" disabled={cart.length < 1} onClick={() => pay("card")}>Оплата картою 💳</button>
        </div>
        </>
    )
}