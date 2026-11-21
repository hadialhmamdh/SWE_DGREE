import {
    AppExp, CExp, Exp, isAppExp, isAtomicExp, isBoolExp, isCExp, isDefineExp,
    isDictArray, isDictExp, isIfExp, isLetExp, isLitExp, isNumExp, isProcExp,
    isStrExp, isVarRef, makeAppExp, makeBinding, makeDefineExp, makeIfExp,
    makeLetExp, makeLitExp, makeProcExp, makeProgram, makeVarRef, parseL32, Program,
    DictArray, DictExp
} from './L32/L32-ast';
import {
    CompoundSExp, EmptySExp, isEmptySExp, makeCompoundSExp, makeEmptySExp,
    makeSymbolSExp, SExpValue
} from './L32/L32-value';
import { map } from 'ramda';
import { isOk } from './shared/result';
import { isPrimOp } from './L3/L3-ast';

// Helper definitions needed for L3 behavior
const helperCode = `
(L32
  (define symbol?
    (lambda (x)
      (not (or (number? x) (boolean? x) (string? x)))))

  (define dict?
    (lambda (d)
      (if (eq? d '())
          #t
          (if (pair? d)
              (if (pair? (car d))
                  (if (symbol? (car (car d)))
                      (dict? (cdr d))
                      #f)
                  #f)
              #f))))

  (define get
    (lambda (d k)
      (if (not (dict? d))
          (error k)
          (if (eq? d '())
              (error k)
              (if (eq? (car (car d)) k)
                  (cdr (car d))
                  (get (cdr d) k)))))) 

  (define dict
    (lambda (d)
      (lambda (k)
        (get d k))))

  (define error
    (lambda (msg) #f))
)
`;

/*
Purpose: rewrite all occurrences of DictExp in a program to AppExp.
Signature: Dict2App (exp)
Type: Program -> Program
*/
export const Dict2App = (exp: Program): Program => {
    const helper = parseL32(helperCode);
    const transformed = exp.exps.map(rewriteDictExp);
    return isOk(helper) ? makeProgram([...helper.value.exps, ...transformed]) : makeProgram(transformed);
};

/*
Purpose: Transform L32 program to L3
Signature: L32ToL3(prog)
Type: Program -> Program
*/

export const L32toL3 = (prog: Program): Program => Dict2App(prog);

// Recursive rewriting
const rewriteDictExp = (exp: Exp): Exp =>
    isDefineExp(exp) ? makeDefineExp(exp.var, rewriteDictCExp(exp.val)) :
    isCExp(exp) ? rewriteDictCExp(exp) :
    exp;

const rewriteDictCExp = (exp: CExp): CExp =>
    isAtomicExp(exp) || isLitExp(exp) ? exp :
    isIfExp(exp) ? makeIfExp(rewriteDictCExp(exp.test), rewriteDictCExp(exp.then), rewriteDictCExp(exp.alt)) :
    isAppExp(exp) ? makeAppExp(rewriteDictCExp(exp.rator), exp.rands.map(rewriteDictCExp)) :
    isProcExp(exp) ? makeProcExp(exp.args, exp.body.map(rewriteDictCExp)) :
    isLetExp(exp) ? makeLetExp(exp.bindings.map(b => makeBinding(b.var.var, rewriteDictCExp(b.val))), exp.body.map(rewriteDictCExp)) :
    isDictArray(exp) ? dictArrayToApp(exp) :
    exp;

// DictArray to AppExp(dict '(...)) transformation
const dictArrayToApp = (dict: DictArray): AppExp =>
    makeAppExp(makeVarRef("dict"), [makeLitExp(dictArrayToSExp(dict))]);

const dictArrayToSExp = (dict: DictArray): CompoundSExp | EmptySExp => {
    const pairs = dict.val.map(pairToCompound);
    const buildList = (ps: CompoundSExp[]): CompoundSExp | EmptySExp =>
        ps.length === 0 ? makeEmptySExp() : makeCompoundSExp(ps[0], buildList(ps.slice(1)));
    return buildList(pairs);
};

const pairToCompound = (d: DictExp): CompoundSExp =>
    makeCompoundSExp(d.var, cexpToSExp(d.val));

// CExp to SExpValue translation for quoted representation
const listToCompound = (vals: SExpValue[]): SExpValue =>
    vals.length === 0 ? makeEmptySExp() : makeCompoundSExp(vals[0], listToCompound(vals.slice(1)));

const cexpToSExp = (exp: CExp): SExpValue =>
    isNumExp(exp) ? exp.val :
    isBoolExp(exp) ? exp.val :
    isStrExp(exp) ? exp.val :
    isVarRef(exp) ? makeSymbolSExp(exp.var) :
    isPrimOp(exp) ? makeSymbolSExp(exp.op) :
    isLitExp(exp) ? exp.val :
    isIfExp(exp)
        ? listToCompound([
            makeSymbolSExp("if"),
            cexpToSExp(exp.test),
            cexpToSExp(exp.then),
            cexpToSExp(exp.alt)
        ]) :
    isAppExp(exp)
        ? listToCompound([
            cexpToSExp(exp.rator),
            ...exp.rands.map(cexpToSExp)
        ]) :
    isProcExp(exp)
        ? listToCompound([
            makeSymbolSExp("lambda"),
            listToCompound(exp.args.map(a => makeSymbolSExp(a.var))),
            ...exp.body.map(cexpToSExp)
        ]) :
    isLetExp(exp)
        ? listToCompound([
            makeSymbolSExp("let"),
            listToCompound(
                exp.bindings.map(b =>
                    listToCompound([
                        makeSymbolSExp(b.var.var),
                        cexpToSExp(b.val)
                    ])
                )
            ),
            ...exp.body.map(cexpToSExp)
        ]) :
    isDictArray(exp)
        ? listToCompound([
            makeSymbolSExp("dict"),
            ...exp.val.map(({ var: k, val }) =>
                listToCompound([k, cexpToSExp(val)])
            )
        ]) :
    makeSymbolSExp("unknown");

