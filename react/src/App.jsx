import React from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import Layout from "./components/layout";
import Home from "./pages/home.jsx";
import Register from "./pages/register.jsx";
import Login from "./pages/login.jsx";
import Alert from "./components/alert.jsx";
import Profile from "./pages/profile.jsx";
import CashRegister from "./pages/cash-register.jsx";
import Products from "./pages/products.jsx";
import Sale from "./pages/sale.jsx";
import Reports from "./pages/reports.jsx";
import ViewReport from "./pages/view-report.jsx";
import CashRegisterV2 from "./pages/cash-register-v2.jsx";
import "./App.css";

function App() {
  return (
    <Router>
      <Alert />
      <Layout>
        <Routes>
          <Route path="/" element={<Home />}></Route>
          <Route path="/register" element={<Register />}></Route>
          <Route path="/login" element={<Login />}></Route>
          <Route path="/profile" element={<Profile />}></Route>
          <Route path="/cash" element={<CashRegister />}/>
          <Route path="/cash_v2" element={<CashRegisterV2 />}/>
          <Route path="/products" element={<Products />}/>
          <Route path="/sale" element={<Sale />} />
          <Route path="/reports" element={<Reports />} />
          <Route path="/reports/:id" element={<ViewReport />} />
        </Routes>
      </Layout>
    </Router>
  );
}

export default App;
