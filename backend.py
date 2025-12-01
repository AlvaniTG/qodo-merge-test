from flask import Flask, request, jsonify
import sqlite3
import os

app = Flask(__name__)

# ==========================================
# SCENARIUSZ A: "Brzydki Kod" (Security & Bugs)
# Cel: Qodo Merge powinno wykryć SQL Injection i hasło w kodzie
# ==========================================

@app.route('/login', methods=['POST'])
def login():
    data = request.json
    username = data.get('username')
    password = data.get('password')

    # BŁĄD 1: Hardcoded password (Backdoor)
    if username == "admin" and password == "SuperSecret123!":
        return jsonify({"message": "Zalogowano administratora (backdoor)"})

    # Symulacja bazy danych (w pliku, żeby działało od razu)
    conn = sqlite3.connect('users.db')
    cursor = conn.cursor()

    try:
        # BŁĄD 2: Klasyczny SQL Injection
        # Qodo powinno krzyczeć o użyciu f-stringa w zapytaniu SQL
        query = f"SELECT * FROM users WHERE username = '{username}' AND password = '{password}'"
        
        # Wykonanie niebezpiecznego zapytania
        cursor.execute(query)
        user = cursor.fetchone()
        
        if user:
            return jsonify({"message": "Zalogowano pomyślnie"})
        else:
            return jsonify({"message": "Błąd logowania"}), 401
    except Exception as e:
        return jsonify({"error": str(e)}), 500
    finally:
        conn.close()

# ==========================================
# SCENARIUSZ B: "Bałaganiarz" (Styl i Czytelność)
# Cel: Qodo Merge powinno zasugerować zmianę nazw zmiennych,
# dodanie type hintów i usunięcie magic numbers.
# ==========================================

@app.route('/complex_calc', methods=['GET'])
def do_calc():
    # Pobranie parametru z URL
    val = int(request.args.get('val', 1))
    
    # BŁĄD: Fatalne nazwy zmiennych, brak docstringów
    x = val
    y = 20
    z = []
    
    # Spaghetti code
    for i in range(100):
        if x > 5:
            # Co to za obliczenie? Nikt nie wie.
            res = (x * y) / 2
            z.append(res)
            x = x - 1
        else:
            z.append(99) # Magic number "99"

    # Zwracanie dziwnego formatu
    return jsonify({"result_count": len(z), "first_val": z[0], "status": "ok_computations"})

# ==========================================
# SCENARIUSZ C: "Pytanie do Eksperta"
# Cel: Czysty kod, ale bez testów.
# Zadanie dla Qodo: "Wygeneruj testy jednostkowe (pytest) dla klasy OrderProcessor"
# ==========================================

@app.route('/process_order', methods=['POST'])
def process_order():
    data = request.json
    processor = OrderProcessor()
    final_price = processor.calculate_total(
        price=data.get('price'),
        quantity=data.get('quantity'),
        customer_type=data.get('customer_type')
    )
    return jsonify({"final_price": final_price})

class OrderProcessor:
    """
    Klasa odpowiedzialna za logikę biznesową zamówień.
    Jest napisana poprawnie, ale brakuje jej testów.
    """
    def calculate_total(self, price, quantity, customer_type):
        if price < 0 or quantity < 0:
            raise ValueError("Price and quantity must be non-negative")

        total = price * quantity

        if customer_type == "VIP":
            return total * 0.90  # 10% zniżki
        elif customer_type == "Regular" and total > 1000:
            return total * 0.95  # 5% zniżki
        
        return total

if __name__ == '__main__':
    # Uruchomienie serwera
    app.run(debug=True, port=5000)