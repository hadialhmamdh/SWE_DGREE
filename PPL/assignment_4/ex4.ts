import { error } from "console";
import { forEach } from "ramda";

//Q1
export function all<T>(promises: Array<Promise<T>>): Promise<Array<T>> {
  return new Promise<T[]>((resolve, reject) => {
    const results: T[] = new Array(promises.length); 
    let completedCount = 0;

    promises.forEach((promise, index) => {
      promise
        .then((value) => {
          results[index] = value; 
          completedCount++;
          if (completedCount === promises.length) {
            resolve(results); 
          }
        })
        .catch((err) => {
          reject(err);
        });
    });

    if (promises.length === 0) {
      resolve([]);
    }
  });
}

  
// Q2
export function* Fib1() {
  yield 1;
  let i : number = 0;
  let j : number = 1;
  while(true){
    let temp = i+j ;
    i=j;
    j=temp;
    yield temp;
  }
}


export function* Fib2() {
    let n = 1;
    const i = (1+Math.sqrt(5))/2;
    const j = (1-Math.sqrt(5))/2;
    while(true){
      yield Math.round((Math.pow(i,n)-Math.pow(j,n))/ Math.sqrt(5));
      n=n+1;
    }
  }
