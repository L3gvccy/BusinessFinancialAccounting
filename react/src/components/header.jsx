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

        <div className="collapse navbar-collapse">
          <ul className="navbar-nav me-auto">
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
                  <Link className="nav-link" to="/sales">
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

          <ul className="navbar-nav">
            {user ? (
              <div className="d-flex align-items-center">
                <span className="navbar-text me-2">
                  Привіт, {user.username}
                </span>
                <Link className="btn btn-primary btn-sm me-2" to="/profile">
                  Профіль
                </Link>
                <Link
                  className="btn btn-outline-danger btn-sm me-2"
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
