from persistence import *

import sys

def add_activity(splittedline: list[str]):
    product_id = int(splittedline[0])
    quantity = int(splittedline[1])
    activator_id = int(splittedline[2])
    date = splittedline[3]

    new_activity = Activitie(product_id, quantity, activator_id, date)

    product = repo.execute_command(f"SELECT quantity FROM products WHERE id={product_id}")
    if not product:  
        return

    current_quantity = product[0][0]
    if current_quantity + quantity < 0:  
        return

    repo.activities.insert(new_activity)

    repo.execute_command(f"UPDATE products SET quantity = quantity + {quantity} WHERE id={product_id}")

def main(args: list[str]):
    inputfilename: str = args[1]
    with open(inputfilename) as inputfile:
        for line in inputfile:
            splittedline: list[str] = line.strip().split(", ")
            add_activity(splittedline)  

if __name__ == '__main__':
    main(sys.argv)
