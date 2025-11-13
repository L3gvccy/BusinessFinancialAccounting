import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";

import { showAlert } from "../utils/show-alert";

export default function Header() {
  const [user, setUser] = useState(null);

  function onClickLogout() {
    fetch("http://localhost:5081/api/account/logout", {
      method: "POST",
      credentials: "include",
    }).then(() => {
      setUser(null);
      showAlert("Ви вийшли з системи.", "info");
    });
  }

  useEffect(() => {
    fetch("http://localhost:5081/api/home/me", {
      credentials: "include",
    })
      .then((res) => (res.ok ? res.json() : { isAuthenticated: false }))
      .then((data) => (data.isAuthenticated ? setUser(data) : setUser(null)));
  }, []);

  return (
    <nav className="navbar navbar-expand-lg navbar-light bg-light shadow-sm">
      <div className="container">
        <Link className="navbar-brand fw-bold" to="/">
          Logo
        </Link>

        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarSupportedContent"
          aria-controls="navbarSupportedContent"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        <div className="collapse navbar-collapse" id="navbarSupportedContent">
          <ul className="navbar-nav me-auto mb-2 mb-lg-0">
            <li>
              <Link className="nav-link" to="/">
                Головна
              </Link>
            </li>

            {user && (
              <>
                <li>
                  <Link className="nav-link" to="/cash">
                    Каса
                  </Link>
                </li>
                <li>
                  <Link className="nav-link" to="/products">
                    Товари
                  </Link>
                </li>
                <li>
                  <Link className="nav-link" to="/sale">
                    Продаж
                  </Link>
                </li>
                <li>
                  <Link className="nav-link" to="/reports">
                    Звіти
                  </Link>
                </li>
              </>
            )}
          </ul>

          <ul className="navbar-nav ms-auto">
            {user ? (
              <div className="d-flex align-items-center flex-wrap">
                <span className="navbar-text me-2">
                  Привіт, {user.username}
                </span>
                <Link className="btn btn-primary btn-sm me-2" to="/profile">
                  Профіль
                </Link>
                <Link
                  className="btn btn-outline-danger btn-sm"
                  to="/"
                  onClick={onClickLogout}
                >
                  Вийти
                </Link>
              </div>
            ) : (
              <>
                <Link className="btn btn-primary btn-sm me-2" to="/login">
                  Увійти
                </Link>
                <Link className="btn btn-outline-primary btn-sm" to="/register">
                  Реєстрація
                </Link>
              </>
            )}
          </ul>
        </div>
      </div>
    </nav>
  );
}
