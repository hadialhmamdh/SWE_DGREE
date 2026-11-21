import { map } from "ramda";
import {
    Exp, isProgram, Program, isBoolExp, isNumExp, isVarRef, isPrimOp,
    isProcExp, isIfExp, isAppExp, isDefineExp, PrimOp, CExp
} from './L3/L3-ast';
import { Result, makeFailure, makeOk, bind, mapResult, safe2 } from './shared/result';


/*
Purpose: Transform L2 AST to JavaScript program string
Signature: l2ToJS(l2AST)
Type: [EXP | Program] => Result<string>
*/export const l2ToJS = (exp: Exp | Program): Result<string> =>
    isProgram(exp) ?
        bind(mapResult(l2ToJS, exp.exps), exps =>
            makeOk(exps.map((e, i) => i < exps.length - 1 ? withSemi(e) : e).join("\n"))) :

    isBoolExp(exp) ? makeOk(`${exp.val}`) :
    isNumExp(exp) ? makeOk(`${exp.val}`) :
    isVarRef(exp) ? makeOk(exp.var) :
    isPrimOp(exp) ? makeOk(mapPrimOp(exp.op)) :

    isProcExp(exp) ?
        bind(l2ToJS(exp.body[exp.body.length - 1]), body =>
            makeOk(`((${exp.args.map(a => a.var).join(",")}) => ${body})`)) :

    isIfExp(exp) ?
        bind(l2ToJS(exp.test), test =>
            bind(l2ToJS(exp.then), then =>
                bind(l2ToJS(exp.alt), alt =>
                    makeOk(`(${test} ? ${then} : ${alt})`)))) :

    isAppExp(exp) ? (
        isPrimOp(exp.rator) ?
            appPrimOpToJS(exp.rator, exp.rands) :
            safe2((rator: string, rands: string[]) => makeOk(`${rator}(${rands.join(",")})`))
                (l2ToJS(exp.rator), mapResult(l2ToJS, exp.rands))
    ) :

    isDefineExp(exp) ?
        bind(l2ToJS(exp.val), val => makeOk(`const ${exp.var.var} = ${val}`)) :

    makeFailure("Unknown expression");

const mapPrimOp = (op: string): string =>
    op === "=" || op === "eq?" ? "===" :
    op === "number?" ? "((x) => typeof(x) === 'number')" :
    op === "boolean?" ? "((x) => typeof(x) === 'boolean')" :
    op;

const appPrimOpToJS = (rator: PrimOp, rands: CExp[]): Result<string> =>
    rator.op === "not" ?
        bind(l2ToJS(rands[0]), rand => makeOk(`(!${rand})`)) :
    rator.op === "number?" || rator.op === "boolean?" ?
        bind(l2ToJS(rands[0]), rand => makeOk(`${mapPrimOp(rator.op)}(${rand})`)) :
        bind(mapResult(l2ToJS, rands), rands =>
            makeOk(`(${rands.join(` ${mapPrimOp(rator.op)} `)})`));

const withSemi = (s: string): string => s.trim().endsWith(";") ? s : `${s};`;
