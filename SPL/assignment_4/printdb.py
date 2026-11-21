from persistence import *

def print_table(table_name, order_by):
    rows = repo.execute_command(f"SELECT * FROM {table_name} ORDER BY {order_by}")
    print(f"{table_name}")
    for row in rows:
        print(row)
    print()

def print_employees_report():
    query = """
        SELECT e.name, e.salary, b.location, 
               COALESCE(SUM(-a.quantity * p.price), 0) as total_income
        FROM employees e
        JOIN branches b ON e.branche = b.id
        LEFT JOIN activities a ON e.id = a.activator_id AND a.quantity < 0
        LEFT JOIN products p ON a.product_id = p.id
        GROUP BY e.id
        ORDER BY e.name
    """
    rows = repo.execute_command(query)
    print("Employees report")
    for row in rows:
        print(" ".join(map(str, row)))
    print()

def print_activities_report():
    query = """
        SELECT a.date, p.description, a.quantity, 
               CASE WHEN a.quantity < 0 THEN e.name ELSE 'None' END AS seller,
               CASE WHEN a.quantity > 0 THEN s.name ELSE 'None' END AS supplier
        FROM activities a
        JOIN products p ON a.product_id = p.id
        LEFT JOIN employees e ON a.activator_id = e.id AND a.quantity < 0
        LEFT JOIN suppliers s ON a.activator_id = s.id AND a.quantity > 0
        ORDER BY a.date
    """
    rows = repo.execute_command(query)
    print("Activities report")
    if rows:
        for row in rows:
            print(row)
        print()

def main():
    tables = [
        ("activities", "date"),
        ("branches", "id"),
        ("employees", "id"),
        ("products", "id"),
        ("suppliers", "id")
    ]
    for table, order_by in tables:
        print_table(table, order_by)
    
    print_employees_report()
    print_activities_report()

if __name__ == '__main__':
    main()
