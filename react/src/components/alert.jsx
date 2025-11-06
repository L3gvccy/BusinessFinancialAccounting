import { useEffect, useState } from "react";

export default function Alert() {
  const [alert, setAlert] = useState(null);

  useEffect(() => {
    const saved = localStorage.getItem("alert");
    if (saved) {
      const { message, type } = JSON.parse(saved);
      setAlert({ msg: message, type });
      localStorage.removeItem("alert");
    }

    function handleAlert(e) {
      const { message, type } = e.detail;
      setAlert({ msg: message, type });
      const timer = setTimeout(() => setAlert(null), 4000);
      return () => clearTimeout(timer);
    }

    window.addEventListener("show-alert", handleAlert);
    return () => window.removeEventListener("show-alert", handleAlert);
  }, []);

  if (!alert) return null;

  return (
    <div
      className={`alert alert-${alert.type} alert-dismissible fade show`}
      style={{
        position: "fixed",
        top: "10px",
        right: "10px",
        zIndex: 9999,
        minWidth: "250px",
      }}
      role="alert"
    >
      {alert.msg}
      <button
        type="button"
        className="btn-close"
        onClick={() => setAlert(null)}
      ></button>
    </div>
  );
}
