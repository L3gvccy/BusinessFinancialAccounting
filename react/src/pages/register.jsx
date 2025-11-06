import React, { useState } from "react";
import { Link } from "react-router-dom";
import axios from "axios";

import { showAlert } from "../utils/show-alert.js";

export default function Register() {
  const [formData, setFormData] = useState({
    username: "",
    fullName: "",
    password: "",
    confirmPassword: "",
    phone: "",
    email: "",
    cashBalance: 0,
    cardBalance: 0,
  });

  const [usernameError, setUsernameError] = useState("");
  const [passwordError, setPasswordError] = useState("");
  const [repeatPasswordError, setRepeatPasswordError] = useState("");
  const [phoneError, setPhoneError] = useState("");
  const [emailError, setEmailError] = useState("");

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleRegisterClick = async (e) => {
    e.preventDefault();

    const passwordRegex = /^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,16}$/;
    if (passwordRegex.test(formData.password) === false) {
      setPasswordError(
        "Пароль повинен містити від 8 до 16 символів, принаймні одну Заглавну букву та одну цифру"
      );
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      setRepeatPasswordError("Паролі не співпадають");
      return;
    }

    const phoneRegex = /^\+380\d{9}$/;
    if (phoneRegex.test(formData.phone) === false) {
      setPhoneError("Телефон повинен бути у форматі +380XXXXXXXXX");
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (emailRegex.test(formData.email) === false) {
      setEmailError("Некоректний формат email");
      return;
    }

    axios
      .post("http://localhost:5081/api/account/register", formData, {
        headers: { "Content-Type": "application/json" },
        withCredentials: true,
      })
      .then((response) => {
        window.location.href = "/login";
        showAlert("Реєстрація успішна! Увійдіть в ваш акаунт", "success");
      })
      .catch((error) => {
        if (error.response && error.response.status === 400) {
          setUsernameError(
            error.response.data?.error || "Користувач з таким логіном вже існує"
          );
        } else {
          setUsernameError("Помилка сервера");
        }
      });
  };

  const googleLogin = () => {
    window.location.href =
      "http://localhost:5001/Account/ExternalLogin?provider=Google";
  };

  return (
    <div className="d-block mx-auto my-3 p-3" style={{ maxWidth: "500px" }}>
      <h2 className="text-center my-3">Реєстрація</h2>

      <form style={{ width: "100%" }} onSubmit={handleRegisterClick}>
        <fieldset className="border rounded-3 p-3 mb-4">
          <legend className="float-none w-auto px-2 fs-5">
            Дані користувача
          </legend>

          <div className="mb-3">
            <label htmlFor="username" className="form-label">
              Логін
            </label>
            <input
              name="username"
              className="form-control"
              value={formData.username}
              onChange={handleChange}
            />
            <span className="text-danger small">{usernameError}</span>
          </div>

          <div className="mb-3">
            <label htmlFor="fullName" className="form-label">
              ПІБ
            </label>
            <input
              name="fullName"
              className="form-control"
              value={formData.fullName}
              onChange={handleChange}
            />
          </div>

          <div className="mb-3">
            <label htmlFor="password" className="form-label">
              Пароль
            </label>
            <input
              name="password"
              type="password"
              className="form-control"
              value={formData.password}
              onChange={handleChange}
            />
            <span className="text-danger small">{passwordError}</span>
          </div>

          <div className="mb-3">
            <label htmlFor="confirmPassword" className="form-label">
              Підтвердження пароля
            </label>
            <input
              name="confirmPassword"
              type="password"
              className="form-control"
              value={formData.confirmPassword}
              onChange={handleChange}
            />
            <span className="text-danger small">{repeatPasswordError}</span>
          </div>

          <div className="mb-3">
            <label htmlFor="phone" className="form-label">
              Телефон
            </label>
            <input
              name="phone"
              className="form-control"
              value={formData.phone}
              onChange={handleChange}
            />
            <span className="text-danger small">{phoneError}</span>
          </div>

          <div className="mb-3">
            <label htmlFor="email" className="form-label">
              Email
            </label>
            <input
              name="email"
              type="email"
              className="form-control"
              value={formData.email}
              onChange={handleChange}
            />
            <span className="text-danger small">{emailError}</span>
          </div>
        </fieldset>

        <fieldset className="border rounded-3 p-3 mb-4">
          <legend className="float-none w-auto px-2 fs-5">
            Стартовий капітал
          </legend>

          <div className="mb-3">
            <label htmlFor="cashBalance" className="form-label">
              Готівка
            </label>
            <input
              name="cashBalance"
              type="number"
              className="form-control"
              value={formData.cashBalance}
              onChange={handleChange}
            />
          </div>

          <div className="mb-3">
            <label htmlFor="cardBalance" className="form-label">
              Карта
            </label>
            <input
              name="cardBalance"
              type="number"
              className="form-control"
              value={formData.cardBalance}
              onChange={handleChange}
            />
          </div>
        </fieldset>

        <button type="submit" className="btn btn-primary w-100">
          Зареєструватися
        </button>

        <p className="text-center mt-3 mb-2">Вже маєте акаунт?</p>
        <div className="d-flex justify-content-center">
          <Link to="/login">Увійти</Link>
        </div>
      </form>

      <h4>Або увійдіть через:</h4>

      <button onClick={googleLogin} className="btn btn-danger w-100">
        <i className="fab fa-google"></i> Google
      </button>
    </div>
  );
}
