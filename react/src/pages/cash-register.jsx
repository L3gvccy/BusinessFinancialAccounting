import axios from "axios";
import React, { useEffect, useState } from "react";
import TransactionModal from "../components/transaction-modal";

export default function CashRegister() {
    const API = "http://localhost:5081/api"
    const [balance, setBalance] = useState({
        "cash": 0,
        "card": 0
    })

    const fetch = async () => {
            try {
                const res = await axios.get(`${API}/cash/cashregister`, {withCredentials: true}) 
                setBalance(res.data)
            } catch (error) {
                if (error.response?.status == 401) {
                    showAlert("Потрібно увійти", "warning");
                    window.location.href = "/login";
                }
            }
        }

    useEffect(() => {
        document.title = "Баланс"
        fetch()
    }, [])

    const [showModal, setShowModal] = useState(false);
    const [accountType, setAccountType] = useState("");
    const [actionType, setActionType] = useState("");

    const handleOpenModal = (action, account) => {
        setActionType(action);
        setAccountType(account);
        setShowModal(true);
    };

    useEffect(() => {
        fetch()
    }, [showModal])

    const handleCloseModal = () => setShowModal(false);

    return (
        <>
            <h2 className="text-center">Баланс</h2>

            <div className="row justify-content-between ">
                <div className="col-md-6 p-3">
                    <div className="card shadow-sm border border-success">
                        <div className="card-body text-center">
                            <h2 className="card-title">Готівковий баланс 💵</h2>
                            <h1 className="display-6 fw-bold text-success">{balance.cash} ₴</h1>

                            <div className="d-flex justify-content-center gap-3 mt-3">
                                <button 
                                type="button" 
                                className="btn btn-success w-50" 
                                onClick={() => handleOpenModal("deposit", "cash")}>
                                    Внести
                                </button>
                                <button
                                type="button"
                                className="btn btn-outline-success w-50"
                                onClick={() => handleOpenModal("withdraw", "cash")}>
                                    Видати
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
                <div className="col-md-6 p-3">
                    <div className="card shadow-sm border border-primary">
                        <div className="card-body text-center">
                            <h2 className="card-title">Баланс картки 💳</h2>
                            <h1 className="display-6 fw-bold text-primary">{balance.card} ₴</h1>

                            <div className="d-flex justify-content-center gap-3 mt-3">
                                <button 
                                type="button" 
                                className="btn btn-primary w-50" 
                                onClick={() => handleOpenModal("deposit", "card")}>
                                    Внести
                                </button>
                                <button
                                type="button"
                                className="btn btn-outline-primary w-50"
                                onClick={() => handleOpenModal("withdraw", "card")}>
                                    Видати
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <TransactionModal show={showModal} onClose={handleCloseModal} accountType={accountType} actionType={actionType} />
        </>
    )
}