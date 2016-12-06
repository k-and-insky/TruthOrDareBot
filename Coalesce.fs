namespace TruthOrDareBot

open System

module Coalesce =
    let (|?) lhs rhs = (if lhs = null then rhs else lhs)