export function showAlert(message, type = "info") {
  localStorage.setItem("alert", JSON.stringify({ message, type }));
  window.dispatchEvent(
    new CustomEvent("show-alert", { detail: { message, type } })
  );
}
