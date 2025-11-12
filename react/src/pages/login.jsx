import { use, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import React from "react";
import axios from "axios";

import { showAlert } from "../utils/show-alert.js";

export default function Login() {
  const [formData, setFormData] = useState({
    username: "",
    password: "",
  });

  const [usernameError, setUsernameError] = useState("");

  function handleSubmit(event) {
    event.preventDefault();

    console.log(formData);

    axios
      .post("http://localhost:5081/api/account/login", formData, {
        headers: { "Content-Type": "application/json" },
        withCredentials: true,
      })
      .then((response) => {
        window.location.href = "/";
        showAlert("Вхід успішний", "success");
      })
      .catch((error) => {
        if (error.response && error.response.status === 400) {
          setUsernameError(
            error.response.data?.error || "Невірний логін або пароль"
          );
        } else {
          setUsernameError("Помилка сервера");
        }
      });
  }

    const handleGoogleLogin = () => {
      window.location.href = "http://localhost:5081/api/account/external-login?provider=Google&returnUrl=/";
    };

    useEffect(() => {
      document.title = "Вхід";

      const params = new URLSearchParams(window.location.search);
      const error = params.get("error");
      if (error === "google_auth_failed") {
        showAlert("Помилка авторизації через Google", "danger");
      }
    }, []);

  return (
    <div className="d-block mx-auto p-3" style={{ maxWidth: "500px" }}>
      <h2 className="text-center my-3">Вхід</h2>

      <form style={{ width: "100%" }} onSubmit={handleSubmit}>
        <fieldset className="border rounded-3 p-3 mb-2">
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
              value={formData.Username}
              onChange={(e) =>
                setFormData({ ...formData, username: e.target.value })
              }
            />
            <span className="text-danger small">{usernameError}</span>
          </div>
          <div className="mb-3">
            <label htmlFor="password" className="form-label">
              Пароль
            </label>
            <input
              name="password"
              type="password"
              className="form-control"
              value={formData.Password}
              onChange={(e) =>
                setFormData({ ...formData, password: e.target.value })
              }
            />
          </div>
          <div className="text-center">
            <button type="submit" className="btn btn-primary px-4 w-100">
              Увійти
            </button>
          </div>
          
        </fieldset>
      </form>

      <p className="text-center mb-2">Не маєте акаунту?</p>
        <div className="d-flex justify-content-center">
          <Link to="/register">Зареєструватися</Link>
        </div>

      <p className="my-2">Або увійдіть через:</p>

      <button onClick={handleGoogleLogin} className="btn btn-danger w-100">
        <i className="fab fa-google"></i> Google
      </button>
    </div>
  );
}
