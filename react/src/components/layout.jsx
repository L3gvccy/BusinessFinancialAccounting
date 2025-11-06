import React from "react";
import Header from "./header.jsx";
import Footer from "./footer.jsx";

export default function Layout({ children }) {
  return (
    <>
      <Header />

      <main className="container my-4">{children}</main>
      <Footer />
    </>
  );
}
