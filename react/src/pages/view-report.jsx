import axios from "axios";
import React from "react";
import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { showAlert } from "../utils/show-alert";
import ReceiptDetails from "../components/receipt-details.jsx";
import { PuffLoader } from 'react-spinners'

export default function ViewReport () {
    const API = "http://localhost:5081/api"
    const { id } = useParams();
    const [loading, setLoading] = useState(true);
    const [report, setReport] = useState(null);
    const [receipts, setReceipts] = useState([]);
    const [selectedReceipt, setSelectedReceipt] = useState(null);

    const getReport = async () => {
        await axios.get(`${API}/reports/view-report/${id}`, {withCredentials: true})
        .then((r) => {
            setReport(r.data.report);
            setReceipts(r.data.receipts);
            console.log(r.data);
            setTimeout(() => {
                setLoading(false);
            }, 500);
        })
        .catch((e) => {
            if (e.response?.status == 404) {
                showAlert("Звіт не знайдено", "warning");
                window.location.href = "/reports";
            } else {
                console.error(e);
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

    useEffect(() => {
        document.title = `Звіт #${id}`;
        getReport()
    }, [])

    return (
        <>
        {loading || report == null 
        ? <div className="d-flex justify-content-center align-items-center" style={{ height: "75vh" }}>
            <PuffLoader
            color="#00fffa"
            size={120}
            />
        </div>
        : <>
            <h3>Звіт #{report.id}</h3>
        <p>Користувач: {report.user.fullName}</p>
        <p>Період: {new Date(report.startDate).toLocaleDateString()} - {new Date(report.endDate).toLocaleDateString()}</p>

        <h4>Підсумки</h4>
        <table className="table table-bordered table-sm w-50">
            <tbody>
                <tr>
                    <th>Продажі готівкою</th>
                    <td>{report.cashSales} ₴</td>
                </tr>
                <tr>
                    <th>Продажі карткою</th>
                    <td>{report.cardSales} ₴</td>
                </tr>
                <tr>
                    <th>Витрати готівкою</th>
                    <td>{report.cashWithdrawals}</td>
                </tr>
                <tr>
                    <th>Витрати карткою</th>
                    <td>{report.cardWithdrawals}</td>
                </tr>
                <tr>
                    <th>Внесення готівкою</th>
                    <td>{report.cashDeposits}</td>
                </tr>
                <tr>
                    <th>Внесення карткою</th>
                    <td>{report.cardDeposits}</td>
                </tr>
                <tr>
                    <th>Прибуток готівкою</th>
                    <td>{report.cashProfit} ₴</td>
                </tr>
                <tr>
                    <th>Прибуток карткою</th>
                    <td>{report.cardProfit} ₴</td>
                </tr>
                <tr>
                    <th>Податок (19,5%)</th>
                    <td>{report.tax} ₴</td>
                </tr>
            </tbody>
        </table>

        <h4>Чеки за період</h4>
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
        </>  
        }
        
        </>
    )
}