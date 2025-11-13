import React from "react";
import { useEffect, useState } from "react";
import axios from "axios";
import { showAlert } from "../utils/show-alert";
import TransactionModalV2 from "../components/transaction-modal-v2";
import coin from "../assets/coin.png";

export default function CashRegisterV2() {
  const API = "http://localhost:5081/api/v2";
  const [balance, setBalance] = useState({
    cash: 0,
    card: 0,
  });

  const getData = async () => {
    await axios
      .get(`${API}/cash/cashregister`, { withCredentials: true })
      .then((r) => {
        setBalance(r.data);
      })
      .catch((e) => {
        if (e.response?.status == 401) {
          showAlert("Потрібно увійти", "warning");
          window.location.href = "/login";
        } else {
          showAlert("Помилка при завантаженні даних", "danger");
        }
      });
  };

  const [selectedAccount, setSelectedAccount] = useState("cash");

  useEffect(() => {
    document.title = "Баланс V2";
    getData();
  }, []);

  const [showModal, setShowModal] = useState(false);
  const [actionType, setActionType] = useState("");

  const handleOpenModal = (action) => {
    setActionType(action);
    setShowModal(true);
  };

  useEffect(() => {
    getData();
  }, [showModal]);

  const handleCloseModal = () => setShowModal(false);

  return (
    <>
      <h2 className="text-center">Баланс</h2>

      <div
        className={`balance-wrapper ${
          selectedAccount == "cash" ? " cash" : " card-wrap"
        }`}
      >
        <div
          className={`balance-inner-card ${
            selectedAccount === "cash" ? "cash" : "card-inner"
          }`}
        >
          <div className="balance-title">
            <div
              className={`slide-text ${
                selectedAccount === "cash" ? "show-cash" : "show-card"
              }`}
            >
              <h2>Готівковий баланс</h2>
              <h2>Баланс картки</h2>
            </div>
          </div>

          <div className="balance-amount">
            <div
              className={`slide-text ${
                selectedAccount === "cash" ? "show-cash" : "show-card"
              }`}
            >
              <p>{balance.cash.toFixed(2)} ₴</p>
              <p>{balance.card.toFixed(2)} ₴</p>
            </div>
          </div>

          <div className="balance-btn-group">
            <button className="operation-btn" onClick={() => handleOpenModal("deposit")}>Внести</button>
            <button className="operation-btn" onClick={() => handleOpenModal("withdraw")}>Видати</button>
          </div>
        </div>

        <div className="balance-img">
          <img src={coin} alt="coin" />
        </div>
      </div>
      <div className="balance-btn-wrapper">
        <button
          className={`balance-btn balance-btn-cash ${
            selectedAccount == "cash" ? " cash-active" : ""
          }`}
          onClick={() => setSelectedAccount("cash")}
        >
          Готівка
        </button>
        <button
          className={`balance-btn balance-btn-card ${
            selectedAccount == "card" ? " card-active" : ""
          }`}
          onClick={() => setSelectedAccount("card")}
        >
          Карта
        </button>
      </div>

      <TransactionModalV2 show={showModal} onClose={handleCloseModal} accountType={selectedAccount} actionType={actionType} />
    </>
  );
}
