import React from "react";
import { useState } from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import Layout from "./components/layout";
import Home from "./pages/home.jsx";
import Register from "./pages/register.jsx";
import Login from "./pages/login.jsx";
import Alert from "./components/alert.jsx";
import Profile from "./pages/profile.jsx";
import CahsRegister from "./pages/cash-register.jsx";

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
          <Route path="/cash" element={<CahsRegister />}/>
        </Routes>
      </Layout>
    </Router>
  );
}

export default App;
