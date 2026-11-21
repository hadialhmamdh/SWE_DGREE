import * as R from "ramda";

const stringToArray = R.split("");

/* Question 1 */
const vowels: string[] = ['a', 'e', 'i', 'o', 'u'];
export const countVowels: (text: string) => number = R.pipe(
  stringToArray,
  R.map(R.toLower),
  R.filter((char: string) => vowels.includes(char)),
  R.length
);
//console.log(countVowels('iiiii         aa&'))

/* Question 2 */
export const isPalindrome: (text: string) => boolean = R.pipe(
    R.toLower,
    R.replace(/[^a-z0-9]/gi, ""), // remove non-alphanumeric characters
    (str: string) => {
      const chars = stringToArray(str);
      const len = chars.length;
      return R.all(
        (i: number) => chars[i] === chars[len - 1 - i],
        R.range(0, Math.floor(len / 2))
      );
    }
  );
  
/* Question 3 */
export type WordTree = {
    root: string;
    children: WordTree[];
}

export const treeToSentence: (tree: WordTree) => string = (tree) => 
    R.pipe(
      (node: WordTree) => [node.root, ...R.chain(treeToSentenceHelper, node.children)],
      R.join(' ')
    )(tree);
  
  const treeToSentenceHelper = (tree: WordTree): string[] => 
    [tree.root, ...R.chain(treeToSentenceHelper, tree.children)];


  /*const t1: WordTree = {
    root: "Hello",
    children: [
      {
        root: "students",
        children: [
          { root: "how", children: [] }
        ]
      },
      { root: "are", children: [] },
      { root: "you?", children: [] }
    ]
  };
  
  console.log(treeToSentence(t1));
  */