import React, { useEffect, useState } from "react";
import axios from "axios";
import { showAlert } from "../utils/show-alert.js";
import { PuffLoader } from 'react-spinners'

const API = "http://localhost:5081/api/account";

export default function Profile() {
  const [loading, setLoading] = useState(true);
  const [profile, setProfile] = useState({
    fullName: "",
    phone: "",
    email: "",
    isGoogleLinked: false,
  });

  const [errors, setErrors] = useState({
    fullName: "",
    phone: "",
    email: "",
    password: "",
  });

  const [passwordData, setPasswordData] = useState({
    newPassword: "",
    confirmPassword: "",
  });

  const loadProfile = async () => {
    try {
      setLoading(true);
      const res = await axios.get(`${API}/profile`, { withCredentials: true });
      setProfile(res.data);
    } catch (e) {
      if (e.response?.status === 401) {
        showAlert("Потрібно увійти", "warning");
        window.location.href = "/login";
      } else {
        showAlert("Не вдалося завантажити профіль", "danger");
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadProfile();
    document.title = "Профіль"
  }, []);

  const handleProfileChange = (e) => {
    setProfile({ ...profile, [e.target.name]: e.target.value });
    setErrors({ ...errors, [e.target.name]: "" });
  };

  const [editingProfile, setEditingProfile] = useState(false);
  const toggleEditingProfile = () => {
    setEditingProfile(!editingProfile);
  }

  const handleSaveProfile = async (e) => {
    e.preventDefault();
    setErrors({ fullName: "", phone: "", email: "", password: "" });
    try {
      await axios.put(`${API}/profile`, {
        fullName: profile.fullName,
        phone: profile.phone,
        email: profile.email,
      }, { withCredentials: true });

      setEditingProfile(false);
      showAlert("Дані профілю успішно оновлено", "success");
      await loadProfile();
    } catch (e) {
      const data = e.response?.data;
      if (data?.details) {
        const detailErrors = {};
        Object.keys(data.details).forEach(k => {
          const arr = data.details[k]?.errors;
          if (arr && arr.length > 0) detailErrors[k.charAt(0).toLowerCase() + k.slice(1)] = arr[0].errorMessage;
        });
        setErrors({ ...errors, ...detailErrors });
      } else {
        showAlert(data?.error || "Помилка збереження профілю", "danger");
      }
    }
  };

  const handlePasswordChange = (e) => {
    setPasswordData({ ...passwordData, [e.target.name]: e.target.value });
    setErrors({ ...errors, password: "" });
  };

  const handleChangePassword = async (e) => {
    e.preventDefault();

    if (passwordData.newPassword.length < 8) {
      setErrors({ ...errors, password: "Пароль повинен містити щонайменше 8 символів" });
      return;
    }
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setErrors({ ...errors, password: "Паролі не співпадають" });
      return;
    }

    try {
      await axios.post(`${API}/change-password`, {
        newPassword: passwordData.newPassword,
        confirmPassword: passwordData.confirmPassword,
      }, { withCredentials: true });

      setPasswordData({ newPassword: "", confirmPassword: "" });
      showAlert("Пароль успішно змінено", "success");
    } catch (e) {
      const data = e.response?.data;
      if (data?.details) {
        setErrors({ ...errors, password: "Некоректні дані пароля" });
      } else {
        showAlert(data?.error || "Помилка зміни пароля", "danger");
      }
    }
  };

  const unlinkGoogle = async () => {
    try {
      await axios.post(`${API}/unlink-google`, {}, { withCredentials: true });
      showAlert("Google авторизацію відв’язано", "success");
      await loadProfile();
    } catch (e) {
      showAlert(e.response?.data?.error || "Не вдалося відв’язати Google", "danger");
    }
  };

  const linkGoogle = async () => {
    window.location.href = `${API}/link-google`;
    showAlert("Google прив'язаний", "success");
  };

  if (loading) {
    return <div className="d-flex justify-content-center align-items-center" style={{ height: "75vh" }}>
                <PuffLoader
                color="#00fffa"
                size={120}
                />
            </div>
  }

  return (
    <div className="d-block mx-auto my-3 p-3" style={{ maxWidth: "500px" }}>
      <h2 className="text-center mb-3">Профіль користувача</h2>

      <h4>Персональні дані</h4>
      <form onSubmit={handleSaveProfile}>
        <div className="mb-3">
          <label className="form-label">ПІБ</label>
          <input
            name="fullName"
            className="form-control"
            value={profile.fullName}
            onChange={handleProfileChange}
            disabled={!editingProfile}
          />
          {!!errors.fullName && <span className="text-danger">{errors.fullName}</span>}
        </div>

        <div className="mb-3">
          <label className="form-label">Телефон</label>
          <input
            name="phone"
            className="form-control"
            value={profile.phone}
            onChange={handleProfileChange}
            disabled={!editingProfile}
          />
          {!!errors.phone && <span className="text-danger">{errors.phone}</span>}
        </div>

        <div className="mb-3">
          <label className="form-label">Email</label>
          <input
            name="email"
            type="email"
            className="form-control"
            value={profile.email}
            onChange={handleProfileChange}
            disabled={!editingProfile}
          />
        {!!errors.email && <span className="text-danger">{errors.email}</span>}
        </div>
        {editingProfile &&
        <button type="submit" className="btn btn-success w-100">Зберегти дані</button>
        }
      </form>
      {!editingProfile &&
      <button type="button" onClick={toggleEditingProfile} className="btn btn-primary w-100">Редагувати дані</button>
      }

      <hr />

      <h4>Зміна паролю</h4>
      <form onSubmit={handleChangePassword}>
        <div className="mb-3">
          <label className="form-label">Новий пароль</label>
          <input
            name="newPassword"
            type="password"
            className="form-control"
            value={passwordData.newPassword}
            onChange={handlePasswordChange}
          />
        </div>

        <div className="mb-3">
          <label className="form-label">Підтвердження пароля</label>
          <input
            name="confirmPassword"
            type="password"
            className="form-control"
            value={passwordData.confirmPassword}
            onChange={handlePasswordChange}
          />
          {!!errors.password && <span className="text-danger">{errors.password}</span>}
        </div>

        <button type="submit" className="btn btn-warning w-100">Змінити пароль</button>
      </form>

      <hr />

      <h4>Google</h4>
      {profile.isGoogleLinked ? (
        <>
          <p>Google акаунт прив’язаний.</p>
          <button onClick={unlinkGoogle} className="btn btn-danger w-100">Відв’язати Google</button>
        </>
      ) : (
        <button onClick={linkGoogle} className="btn btn-primary w-100">Прив’язати Google</button>
      )}
    </div>
  );
}
