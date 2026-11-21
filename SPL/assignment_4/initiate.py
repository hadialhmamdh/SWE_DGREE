from persistence import *

import sys
import os

def add_branche(splittedline : list[str]):
    brancheObject=Branche(splittedline[0],splittedline[1],splittedline[2])
    repo.branches.insert(brancheObject)

def add_supplier(splittedline : list[str]):
    # Check if any employee has this ID
    supplierID = splittedline[0]  
    employee = repo.employees.find(id = supplierID)
    if employee:
       print(f"Error: Supplier ID {supplierID} conflicts with an existing Employee ID. Skipping insertion.")
       return
    supplierObject=Supplier(splittedline[0],splittedline[1],splittedline[2])
    repo.suppliers.insert(supplierObject)

def add_product(splittedline : list[str]):
    productObject=Product(splittedline[0],splittedline[1],splittedline[2],splittedline[3])
    repo.products.insert(productObject)

def add_employee(splittedline : list[str]):
    # Check if any supplier has this ID
    employeeID = splittedline[0]  
    supplier = repo.suppliers.find(id = employeeID)
    if supplier:
       print(f"Error: Employee ID {employeeID} conflicts with an existing Supplier ID. Skipping insertion.")
       return
    employeeObject=Employee(splittedline[0],splittedline[1],splittedline[2],splittedline[3])
    repo.employees.insert(employeeObject)


adders = {  "B": add_branche,
            "S": add_supplier,
            "P": add_product,
            "E": add_employee}

def main(args : list[str]):
    inputfilename = args[1]
    # delete the database file if it exists
    repo._close()
    # uncomment if needed
    if os.path.isfile("bgumart.db"):
         os.remove("bgumart.db")
    repo.__init__()
    repo.create_tables()
    with open(inputfilename) as inputfile:
        for line in inputfile:
            splittedline : list[str] = line.strip().split(",")
            adders.get(splittedline[0])(splittedline[1:])

if __name__ == '__main__':
    main(sys.argv)