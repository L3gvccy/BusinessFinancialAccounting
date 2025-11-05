import React from "react";

export default function Footer() {
  const date = new Date();
  const year = date.getFullYear();
  return (
    <footer className="text-center mt-auto text-muted py-3 bg-light shadow-sm">
      <p>&copy; {year} - Ведення фінансового обліку малого бізнесу</p>
    </footer>
  );
}
