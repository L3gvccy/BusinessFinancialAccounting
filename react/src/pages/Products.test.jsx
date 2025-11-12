import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import axios from 'axios';
import Products from './products.jsx';
import { showAlert } from '../utils/show-alert';

jest.mock('axios');
jest.mock('../utils/show-alert', () => ({
  showAlert: jest.fn()
}));

const originalError = console.error;

beforeAll(() => {
  console.error = (...args) => {
    const first = args[0];
    const isNavigationNotImplemented =
      first &&
      ((typeof first === 'string' && first.includes('Not implemented: navigation')) ||
        (first.message && first.message.includes && first.message.includes('Not implemented: navigation')));

    if (isNavigationNotImplemented) {
      return;
    }

    return originalError.call(console, ...args);
  };
});

afterAll(() => {
  console.error = originalError;
});

describe('Products Component', () => {
  const mockProducts = [
    { id: 1, code: 101, name: 'Product 1', quantity: 10, units: 'шт', price: 100.5 },
    { id: 2, code: 102, name: 'Product 2', quantity: 5, units: 'кг', price: 50.0 }
  ];

  beforeEach(() => {
    jest.clearAllMocks();
  });

  // Завантаження та відображення списку товарів
  test('завантажує та відображає список товарів', async () => {
    axios.get.mockResolvedValueOnce({ data: mockProducts });

    render(<Products />);

    await waitFor(() => {
      expect(screen.getByText('Product 1')).toBeInTheDocument();
      expect(screen.getByText('Product 2')).toBeInTheDocument();
    });

    expect(axios.get).toHaveBeenCalledWith(
      'http://localhost:5081/api/product/products',
      { withCredentials: true }
    );
  });

  // Відображення повідомлення коли список товарів порожній
  test('показує повідомлення коли немає товарів', async () => {
    axios.get.mockResolvedValueOnce({ data: [] });

    render(<Products />);

    await waitFor(() => {
      expect(screen.getByText(/У вас немає жодного товару/i)).toBeInTheDocument();
    });
  });

  // Відкриття форми для додавання нового товару
  test('відкриває форму додавання товару', async () => {
    axios.get.mockResolvedValueOnce({ data: mockProducts });

    render(<Products />);

    const addButton = screen.getByText('Додати товар');
    fireEvent.click(addButton);

    await waitFor(() => {
      const inputs = screen.getAllByRole('spinbutton');
      expect(inputs.length).toBeGreaterThan(0);
    });
  });

  // Додавання нового товару
  test('додає новий товар успішно', async () => {
    axios.get.mockResolvedValueOnce({ data: [] });
    axios.post.mockResolvedValueOnce({ data: { message: 'Товар успішно додано' } });
    axios.get.mockResolvedValueOnce({ data: mockProducts });

    render(<Products />);

    const addButton = screen.getByText('Додати товар');
    fireEvent.click(addButton);

    await waitFor(() => {
      const inputs = screen.getAllByRole('spinbutton');
      expect(inputs.length).toBe(3);
    });

    const numberInputs = screen.getAllByRole('spinbutton');
    const codeInput = numberInputs[0];
    const quantityInput = numberInputs[1];
    const priceInput = numberInputs[2];

    fireEvent.change(codeInput, { target: { name: 'code', value: 103 } });

    const nameInput = screen.getByRole('textbox');
    fireEvent.change(nameInput, { target: { name: 'name', value: 'New Product' } });

    fireEvent.change(quantityInput, { target: { name: 'quantity', value: 15 } });

    const unitsSelect = screen.getByRole('combobox');
    fireEvent.change(unitsSelect, { target: { name: 'units', value: 'шт' } });

    fireEvent.change(priceInput, { target: { name: 'price', value: 99.99 } });

    const saveButton = screen.getByText('✅');
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(axios.post).toHaveBeenCalled();
      expect(showAlert).toHaveBeenCalledWith('Товар успішно додано', 'success');
    });
  });

  // Пошук товару за кодом
  test('шукає товар за кодом', async () => {
    axios.get.mockResolvedValueOnce({ data: mockProducts });
    axios.get.mockResolvedValueOnce({
      data: { code: 101, name: 'Found Product', units: 'шт', price: 100 }
    });

    render(<Products />);

    const addButton = screen.getByText('Додати товар');
    fireEvent.click(addButton);

    await waitFor(() => {
      const inputs = screen.getAllByRole('spinbutton');
      expect(inputs.length).toBeGreaterThan(0);
    });

    const codeInput = screen.getAllByRole('spinbutton')[0];
    fireEvent.change(codeInput, { target: { name: 'code', value: 101 } });

    const searchButton = screen.getByText('🔍');
    fireEvent.click(searchButton);

    await waitFor(() => {
      expect(axios.get).toHaveBeenCalledWith(
        'http://localhost:5081/api/product/find-by-code',
        { params: { code: 101 }, withCredentials: true }
      );
      expect(showAlert).toHaveBeenCalledWith(
        'Товар знайдено, введіть кількість товару!',
        'success'
      );
    });
  });

  // Редагування існуючого товару та збереження змін
  test('редагує товар успішно', async () => {
    axios.get.mockResolvedValueOnce({ data: mockProducts });
    axios.post.mockResolvedValueOnce({ data: { message: 'Товар оновлено' } });
    axios.get.mockResolvedValueOnce({ data: mockProducts });

    render(<Products />);

    await waitFor(() => {
      expect(screen.getByText('Product 1')).toBeInTheDocument();
    });

    const editButtons = screen.getAllByText('✏️');
    fireEvent.click(editButtons[0]);

    await waitFor(() => {
      const nameInput = screen.getByDisplayValue('Product 1');
      expect(nameInput).toBeInTheDocument();
    });

    const nameInput = screen.getByDisplayValue('Product 1');
    fireEvent.change(nameInput, { target: { name: 'name', value: 'Updated Product' } });

    const saveButton = screen.getByText('💾');
    fireEvent.click(saveButton);

    await waitFor(() => {
      expect(axios.post).toHaveBeenCalled();
      expect(showAlert).toHaveBeenCalledWith('Товар оновлено', 'success');
    });
  });
});