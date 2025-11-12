import React from "react";

export default function ReceiptDetails ({ receipt }) {
    return (
        <>
        {receipt == null 
        ? <>
        <h5>Деталі чеку</h5>
        <p><em>Виберіть чек, щоб побачити деталі</em></p>
        </> 
        : <>
        <h5>Деталі чеку #{receipt.id}</h5>
        <p>Метод оплати: <strong>{receipt.paymentMethod == "cash" ? "Готівка" : "Карта"}</strong></p>
        <table className="table">
            <thead>
                <tr>
                    <th>Код</th>
                    <th>Назва</th>
                    <th>Ціна</th>
                    <th>Кількість</th>
                    <th>Сума</th>
                </tr>
            </thead>
            <tbody>
                {receipt?.products?.map((p, index) => (
                <tr key={index}>
                    <td>{p.code}</td>
                    <td>{p.name}</td>
                    <td>{p.price} ₴</td>
                    <td>{p.quantity}</td>
                    <td>{(p.price * p.quantity).toFixed(2)} ₴</td>
                </tr>
                ))}
                <tr className="fw-bold">
                    <td colSpan={4}>Загальна сума:</td>
                    <td>{receipt.totalPrice} ₴</td>
                </tr>
            </tbody>
        </table>
        </>}
        
        </>
    )
}