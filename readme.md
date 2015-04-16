SEScrimplify
============

Script simplifying tool intended for use with Space Engineers' programmable blocks. Acts as a polyfill for language features not yet supported by translating C# constructs into more primitive forms.

Supported rewrites:
 * Extension methods rewritten as static method calls.
 * Lambdas rewritten as struct or static methods.
 
Limitations:
 * Because class declarations are not permitted by SE's scripting environment lambdas are limited to 'pure functional' uses. A parent scope's variables cannot be reassigned from within a lambda, so mutating state is never permitted.
 
Planned:
 * Expansion of namespaces to eliminate 'using' directives, which are illegal in SE scripts.
 * Rewrite 'foreach' as a while loop and enumerator.
 * Detection of attempts to mutate parent scopes and compile-time warnings of the same.

 