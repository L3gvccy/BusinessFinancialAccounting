import axios from "axios";
import React, {useState, useEffect} from "react";
import { showAlert } from "../utils/show-alert";

export default function Products() {
    const API = "http://localhost:5081/api";
    const [products, setProducts] = useState([])
    const [editProduct, setEditProduct] = useState()
    const [addingProduct, setAddingProduct] = useState(false)
    const [productToAdd, setProductToAdd] = useState({
        code: "",
        name: "",
        quantity: "",
        units: "",
        price: ""
    })

    const getProducts = async () => {
        await axios.get(`${API}/product/products`, {withCredentials: true})
        .then((r) => {
            setProducts(r.data)
        })
        .catch((e) => {
            if (e.response?.status == 401) {
                showAlert("Потрібно увійти", "warning");
                window.location.href = "/login";
            }
            else if (e.response?.status == 404) {
                setEditProducts([])
            }
        })
    }

    const checkFields = (p) => {
        if (p.units == "шт" && !Number.isInteger(Number(p.quantity)) || p.quantity < 0) return false
        if (!Number.isInteger(p.code) || p.code <= 0 || p.price <= 0) return false
        if (p.name.length <= 0 || p.code.length <= 0 || p.quantity.length <= 0 || p.units.length <= 0 || p.price.length <= 0) return false
        return true
    }

    // Add product
    const handleAddInput = (e) => {
        const { name, type, value } = e.target
        setProductToAdd({ 
            ...productToAdd, 
            [name]: type === "number"
            ? (value === "" ? "" : Number(value))
            : value
        })
    }

    const findByCode = async () => {
        const code = productToAdd.code
        if (code < 0) {
            showAlert("Код товару введено некоректно!", "danger")
            return
        } 
        await axios.get(`${API}/product/find-by-code`, { params: {code} ,withCredentials: true})
        .then((r) => {
            setProductToAdd(r.data)
            showAlert("Товар знайдено, введіть кількість товару!", "success")
        })
        .catch((e) => {
            if (e.response?.status == 404) {
                showAlert("Товар не знайдено, введіть дані вручну!")
            } else if (e.response?.status == 401) {
                showAlert("Потрібно увійти", "warning");
                window.location.href = "/login";
            }

        })
    }

    const handleSumbitAdd = async () => {
        if (!checkFields(productToAdd)) {
            console.log(productToAdd)
            showAlert("Дані введено некоректно", "danger")
            return
        }

        await axios.post(`${API}/product/add`, productToAdd, {withCredentials: true})
        .then((r) => {
            showAlert(r.data.message, "success")
            setAddingProduct(false)
            setProductToAdd({
                code: "",
                name: "",
                quantity: "",
                units: "",
                price: ""
            })
            getProducts()
        })
        .catch((e) => {
            if (e.response?.status == 401) {
                showAlert("Потрібно увійти", "warning");
                window.location.href = "/login";
            }
        })
    }

    // Edit product
    const handleSetEdit = (p) => {
        setEditProduct(p)
    }

    const handleRemoveEdit = () => {
        setEditProduct()
    }

    const handleEdit = (e) => {
        const { name, type, value } = e.target
        setEditProduct({
            ...editProduct, 
            [name]: type === "number"
            ? (value === "" ? "" : Number(value))
            : value
        })
    }

    const handleSubmitEdit = async () => {
        console.log(editProduct)
        if (!checkFields(editProduct)) {
            showAlert("Дані введено некоректно", "danger")
            return
        }
        await axios.post(`${API}/product/edit`, editProduct, {withCredentials: true})
        .then((r) => {
            showAlert(r.data.message, "success")
            setEditProduct()
            getProducts()
        })
        .catch((e) => {
            if (e.response?.status == 401) {
                showAlert("Потрібно увійти", "warning");
                window.location.href = "/login";
            }
        })
    }


    useEffect(() => {
        getProducts()
    }, [])

    return (
        <>
        <h2 className="text-center">Список товарів</h2>

        <button className="btn btn-success px-4 mb-3" onClick={() => {setAddingProduct(true)}}>Додати товар</button>

        {products.length > 0 || addingProduct ?
        <table className="table table-bordered table-striped">
            <thead>
                <tr>
                    <th className="col-2">Код</th>
                    <th className="col-2">Назва</th>
                    <th className="col-2">Кількість</th>
                    <th className="col-1">Одиниці</th>
                    <th className="col-2">Ціна</th>
                    <th className="col-2">Дії</th>
                </tr>
            </thead>
            <tbody>
                {addingProduct &&
                    <tr>
                        <td>
                            <input className="form-control" type="number" name="code" value={productToAdd.code} onChange={handleAddInput} />
                        </td>
                        <td>
                            <input className="form-control" type="text" name="name" value={productToAdd.name} onChange={handleAddInput} />
                        </td>
                        <td>
                            <input className="form-control" type="number" name="quantity" value={productToAdd.quantity} onChange={handleAddInput} />
                        </td>
                        <td>
                            <select className="form-control" name="units" value={productToAdd.units} onChange={handleAddInput}>
                                <option value="">---</option>
                                <option value="кг">кг</option>
                                <option value="шт">шт</option>
                            </select>
                        </td>
                        <td>
                            <input className="form-control"  type="number" name="price" value={productToAdd.price} onChange={handleAddInput} />
                        </td>
                        <td>
                            <button className="btn btn-sm btn-outline-success me-2" onClick={handleSumbitAdd}>✅</button>
                            <button className="btn btn-sm btn-outline-primary me-2" onClick={findByCode}>🔍</button>
                            <button className="btn btn-sm btn-outline-danger" onClick={() => {setAddingProduct(false)}}>🚫</button>
                        </td>
                    </tr>
                }
                {products.map((p, i) => (
                    editProduct?.id == p.id ?
                    <tr key={p.id || i}>
                        <td>
                            <input className="form-control" type="number" name="code" value={editProduct.code} onChange={handleEdit} />
                        </td>
                        <td>
                            <input className="form-control" type="text" name="name" value={editProduct.name} onChange={handleEdit} />
                        </td>
                        <td>
                            <input className="form-control" type="number" name="quantity" value={editProduct.quantity} onChange={handleEdit} />
                        </td>
                        <td>
                            <select className="form-control" name="units" value={editProduct.units} onChange={handleEdit}>
                                <option value="кг">кг</option>
                                <option value="шт">шт</option>
                            </select>
                        </td>
                        <td>
                            <input className="form-control"  type="number" name="price" value={editProduct.price} onChange={handleEdit} />
                        </td>
                        <td>
                            <button className="btn btn-sm btn-outline-primary me-2" onClick={handleSubmitEdit}>💾</button>
                            <button className="btn btn-sm btn-outline-danger" onClick={handleRemoveEdit}>❌</button>
                        </td>
                    </tr>
                    :
                    <tr key={p.id || i}>
                        <td>{p.code}</td>
                        <td>{p.name}</td>
                        <td>{p.quantity}</td>
                        <td>{p.units}</td>
                        <td>{p.price}</td>
                        <td>
                            <button className="btn btn-sm btn-outline-warning" onClick={() => {handleSetEdit(p)}}>✏️</button>
                        </td>
                        
                    </tr>
                ))}
            </tbody>
        </table>
        :
        <>
            <p className="text-muted">У вас немає жодного товару. Додайти товар, щоб побачити його у списку.</p>
        </>
        }
        </>
    )
}