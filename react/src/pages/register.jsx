import React, { use, useState } from "react";
import { Link } from "react-router-dom";

export default function Register() {
  const [passwordError, setPasswordError] = useState(null);
  function handleRegisterClick(event) {
    const password = event.target.form["Password"].value;
    const confirmPassword = event.target.form["confirmPassword"].value;
    if (password !== confirmPassword) {
      event.preventDefault();
      setPasswordError("Паролі не співпадають");
    } else {
      setPasswordError(null);
    }
  }
  return (
    <div className="d-block mx-auto my-3 p-3" style={{ maxWidth: "500px" }}>
      <h2 className="text-center my-3">Реєстрація</h2>

      <form style={{ width: "100%" }}>
        <fieldset className="border rounded-3 p-3 mb-4">
          <legend className="float-none w-auto px-2 fs-5">
            Дані користувача
          </legend>

          <div className="mb-3">
            <label for="Username" className="form-label">
              Логін
            </label>
            <input asp-for="Username" className="form-control" />
            <span
              asp-validation-for="Username"
              className="text-danger small"
            ></span>
          </div>

          <div className="mb-3">
            <label for="FullName" className="form-label">
              ПІБ
            </label>
            <input asp-for="FullName" className="form-control" />
            <span
              asp-validation-for="FullName"
              className="text-danger small"
            ></span>
          </div>

          <div className="mb-3">
            <label for="Password" className="form-label">
              Пароль
            </label>
            <input
              asp-for="Password"
              type="password"
              className="form-control"
            />
            <span className="text-danger small">{passwordError}</span>
          </div>

          <div className="mb-3">
            <label for="confirmPassword" className="form-label">
              Підтвердження пароля
            </label>
            <input
              name="confirmPassword"
              type="password"
              className="form-control"
            />
          </div>

          <div className="mb-3">
            <label for="Phone" className="form-label">
              Телефон
            </label>
            <input asp-for="Phone" className="form-control" />
            <span
              asp-validation-for="Phone"
              className="text-danger small"
            ></span>
          </div>

          <div className="mb-3">
            <label for="Email" className="form-label">
              Email
            </label>
            <input asp-for="Email" type="email" className="form-control" />
            <span
              asp-validation-for="Email"
              className="text-danger small"
            ></span>
          </div>
        </fieldset>

        <fieldset className="border rounded-3 p-3 mb-4">
          <legend className="float-none w-auto px-2 fs-5">
            Стартовий капітал
          </legend>

          <div className="mb-3">
            <label for="cashBalance" className="form-label">
              Готівка
            </label>
            <input name="cashBalance" type="number" className="form-control" />
          </div>

          <div className="mb-3">
            <label for="cardBalance" className="form-label">
              Карта
            </label>
            <input name="cardBalance" type="number" className="form-control" />
          </div>
        </fieldset>

        <button
          onClick={() => {
            handleRegisterClick();
          }}
          className="btn btn-primary w-100"
        >
          Зареєструватися
        </button>
        <p className="text-center mt-3 mb-2">Вже маєте акаунт?</p>
        <div className="d-flex justify-content-center">
          <Link to="/login">Увійти</Link>
        </div>
      </form>

      <h4>Або увійдіть через:</h4>

      <a
        className="btn btn-danger w-100"
        asp-controller="Account"
        asp-action="ExternalLogin"
        asp-route-provider="Google"
      >
        <i className="fab fa-google"></i> Google
      </a>
    </div>
  );
}
