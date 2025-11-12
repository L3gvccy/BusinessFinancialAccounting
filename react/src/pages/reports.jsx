import React, { useState, useEffect } from "react";
import { showAlert } from "../utils/show-alert";
import axios from "axios";

import ReceiptDetails from "../components/receipt-details.jsx";

export default function Reports () {
    const API = "http://localhost:5081/api"

    const [receipts, setReceipts] = useState([])
    const [reports, setReports] = useState([])
    const [selectedReceipt, setSelectedReceipt] = useState(null)
    const [startDate, setStartDate] = useState(null)
    const [endDate, setEndDate] = useState(null)

    const getData = async () => {
        await axios.get(`${API}/reports/`, {withCredentials: true})
        .then((r) => {
            setReceipts(r.data.receipts)
            setReports(r.data.reports)
        })
        .catch((e) => {
            if (e.response?.status == 401) {
                showAlert("Потрібно увійти", "warning");
                window.location.href = "/login";
            } else {
                showAlert("Помилка при завантаженні даних", "danger");
            }
        })
    }

    const handleReceiptSelect = async (id) => { 
        if (id == selectedReceipt?.id) {
            setSelectedReceipt(null)
            return;
        } else {
            await axios.get(`${API}/reports/get-reciept-details`, {params: {receiptId: id}}, {withCredentials: true})
            .then((r) => {
                setSelectedReceipt(r.data.receipt)
            })
            .catch((e) => {
                showAlert("Помилка при завантаженні даних чеку", "danger");
            })
        }
    }

    const handleStartDateInput = (e) => {
        const { value } = e.target
        setStartDate(value)
    }

    const handleEndDateInput = (e) => {
        const { value } = e.target
        setEndDate(value)
    }

    const generateReport = async () => {
        if (!startDate || !endDate) {
            showAlert("Вкажіть обидві дати", "warning");
            return;
        }
        if (new Date(startDate) > new Date(endDate)) {
            showAlert("Дата початку не може бути більшою за дату кінця", "warning");
            return;
        }
        await axios.post(`${API}/reports/generate-report`, {startDate, endDate}, {withCredentials: true})
        .then((r) => {
            showAlert("Звіт успішно згенеровано", "success");
            getData();
        })
        .catch((e) => {
            showAlert("Помилка при генерації звіту", "danger");
            console.error(e);
        })
    }

    const viewReport = async (id) => {
        window.location.href = `/reports/${id}`;
    }

    const regenerateReport = async (id) => {
        await axios.post(`${API}/reports/regenerate-report/${id}`, {}, {withCredentials: true})
        .then((r) => {
            showAlert(`Звіт #${id} успішно оновлено`, "success");
            getData();
        })
        .catch((e) => {
            showAlert("Помилка при оновленні звіту", "danger");
            console.error(e);
        }
        );
    }

    const deleteReport = async (id) => {
        await axios.post(`${API}/reports/delete-report/${id}`, {}, {withCredentials: true})
        .then((r) => {
            showAlert(`Звіт #${id} успішно видалено`, "success");
            getData();
        })
        .catch((e) => {
            showAlert("Помилка при видаленні звіту", "danger");
            console.error(e);
        });
    }

    useEffect(() => {
        document.title = "Звіти"
        getData()
    }, [])

    return (
        <>
        <h2 className="text-center">Звіти</h2>
        <div className="row">
            <div className="col-auto">
                <label htmlFor="start-date">Дата початку</label>
                <input type="date" name="start-date" className="form-control" onChange={(e) => handleStartDateInput(e)} />
            </div>
            <div className="col-auto">
                <label htmlFor="end-date">Дата кінця</label>
                <input type="date" name="end-date" className="form-control"  onChange={(e) => handleEndDateInput(e)} />
            </div>
            <div className="col-auto align-self-end">
                <button type="button" className="btn btn-primary" onClick={() => generateReport()}>Згенерувати звіт</button>
            </div>
        </div>

        <div className="row mt-3" style={{ height: "50vh" }}>
            <div className="col-4" style={{ height: "50vh" }}>
                <h5>Список чеків</h5>
                <ul className="list-group overflow-auto" style={{ maxHeight: "calc(50vh - 32px)" }}>
                {receipts.map((receipt) => {
                    return (
                    <li className={`list-group-item d-flex justify-content-between align-items-center` + (selectedReceipt?.id == receipt.id ? " bg-light" : "")}
                    key={receipt.id}
                    onClick={() => handleReceiptSelect(receipt.id)}
                    style={{cursor: "pointer"}}>
                        <span>#{receipt.id} - {new Date(receipt.timeStamp).toLocaleString()}</span>
                        <span>Сума: {receipt.totalPrice} ₴</span>
                    </li>
                    )
                })}
                </ul>
            </div>
            <div className="col-8">
                <ReceiptDetails receipt={selectedReceipt} />
            </div>
        </div>

        <h4 className="text-center mt-3">Сформовані звіти</h4>
        <table className="table table-striped">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Дата початку</th>
                    <th>Дата кінця</th>
                    <th>Прибуток готівкою</th>
                    <th>Прибуток карткою</th>
                    <th>Податок</th>
                    <th>Дії</th>
                </tr>
            </thead>
            <tbody>
                {!reports.length && (
                    <tr>
                        <td colSpan="7" className="text-center">Немає звітів</td>
                    </tr>
                )}
                {reports.map((report) => {
                    return (
                    <tr key={report.id}>
                        <td>{report.id}</td>
                        <td>{new Date(report.startDate).toLocaleDateString()}</td>
                        <td>{new Date(report.endDate).toLocaleDateString()}</td>
                        <td>{report.cashSales} ₴</td>
                        <td>{report.cardSales} ₴</td>
                        <td>{report.tax} ₴</td>
                        <td>
                            <button className="btn btn-sm btn-primary me-2" onClick={() => viewReport(report.id)}>📄</button>
                            <button className="btn btn-sm btn-warning me-2" onClick={() => regenerateReport(report.id)}>🔄</button>
                            <button className="btn btn-sm btn-danger" onClick={() => deleteReport(report.id)}>🗑️</button>
                        </td>
                    </tr>
                    )
                })}
            </tbody>

        </table>
        </>
    )
}