import React, { useEffect, useState } from "react";
import { showAlert } from "../utils/show-alert";
import axios from "axios";

export default function TransactionModal({ show, onClose, accountType, actionType }) {
  const API = "http://localhost:5081/api";
  const [modal, setModal] = useState({
    accountType,
    actionType,
    moneyAmount: 0,
  });
  const [errorMsg, setErrorMsg] = useState("");

  useEffect(() => {
    setModal((prev) => ({ ...prev, accountType, actionType }));
  }, [accountType, actionType]);

  useEffect(() => {
    const modalEl = document.getElementById("transactionModal");
    if (!modalEl || !window.bootstrap) return;

    const modalInstance = window.bootstrap.Modal.getOrCreateInstance(modalEl);
    if (show) modalInstance.show();
    else modalInstance.hide();

    modalEl.addEventListener("hidden.bs.modal", onClose);
    return () => modalEl.removeEventListener("hidden.bs.modal", onClose);
  }, [show, onClose]);

  const handleAmountChange = (e) => {
    setModal({ ...modal, moneyAmount: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const amount = parseFloat(modal.moneyAmount);

    if (isNaN(amount) || amount <= 0) {
      return setErrorMsg("Сума внесення/видачі має бути більшою за 0!");
    }
    setErrorMsg("");

    try {
      const res = await axios.post(`${API}/cash/transaction`, modal, {
        withCredentials: true,
      });
      setModal({...modal, moneyAmount: 0})
      onClose();
      showAlert(res.data?.message, "success")
    } catch (err) {
      console.error(err);
      setErrorMsg("Помилка виконання операції");
    }
  };

  const title =
    actionType === "deposit"
      ? `Внести ${accountType === "cash" ? "готівку" : "на картку"}`
      : `Видати ${accountType === "cash" ? "готівку" : "з картки"}`;
  const btnColor = accountType === "cash" ? "success" : "primary";

  return (
    <div
      className="modal fade"
      id="transactionModal"
      tabIndex="-1"
      aria-hidden="true"
    >
      <div className="modal-dialog">
        <div className="modal-content border-2">
          <div className="modal-header">
            <h5 className="modal-title">{title}</h5>
            <button
              type="button"
              className="btn-close"
              data-bs-dismiss="modal"
              aria-label="Close"
            ></button>
          </div>

          <div className="modal-body">
            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label className="form-label">Сума</label>
                <input
                  type="number"
                  className="form-control"
                  value={modal.moneyAmount}
                  onChange={handleAmountChange}
                  required
                />
                {errorMsg && (
                  <div className="text-danger mt-2">{errorMsg}</div>
                )}
              </div>

              <button type="submit" className={`btn btn-${btnColor} w-100`}>
                {actionType === "deposit"
                  ? "Підтвердити внесення"
                  : "Підтвердити видачу"}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}
