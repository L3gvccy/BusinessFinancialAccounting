import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";

export default function CardFeature({ title, description, link }) {
  const [auth, setAuth] = useState(false);

  useEffect(() => {
    fetch("api/home/me")
      .then((r) => r.json())
      .then((data) => setAuth(data.isAuthenticated));
  });

  return (
    <div className="col-md 4">
      <div className="card shadow-sm border-0 rounded">
        <div className="card-body text-center">
          <h5 className="card-title">{title}</h5>
          <p className="card-text">{description}</p>
          {auth ? (
            <Link to={link} className="btn btn-primary px-3">
              Перейти
            </Link>
          ) : (
            <Link to="/login" className="btn btn-primary px-3">
              Увійти
            </Link>
          )}
        </div>
      </div>
    </div>
  );
}
