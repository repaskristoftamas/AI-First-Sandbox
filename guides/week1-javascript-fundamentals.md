# Week 1 -- JavaScript Fundamentals

A C#-to-JavaScript bridge. Each section shows the JS concept alongside the C# equivalent you already know.

---

## Table of Contents

1. [Variables: `let`, `const`, `var`](#1-variables-let-const-var)
2. [Arrow Functions](#2-arrow-functions)
3. [Template Literals](#3-template-literals)
4. [Destructuring](#4-destructuring)
5. [Spread and Rest (`...`)](#5-spread-and-rest-)
6. [Modules (`import` / `export`)](#6-modules-import--export)
7. [Array Methods (LINQ Equivalents)](#7-array-methods-linq-equivalents)
8. [Closures](#8-closures)
9. [Higher-Order Functions](#9-higher-order-functions)
10. [Callback Patterns](#10-callback-patterns)
11. [Short-Circuit Evaluation (`&&`, `??`, `?.`)](#11-short-circuit-evaluation---)
12. [Key Differences from C# to Keep in Mind](#12-key-differences-from-c-to-keep-in-mind)

---

## 1. Variables: `let`, `const`, `var`

### C# Comparison

| C# | JavaScript | Scope |
|---|---|---|
| `int x = 5;` (reassignable) | `let x = 5;` | Block-scoped |
| `const int X = 5;` | `const x = 5;` | Block-scoped, no reassignment |
| _(no equivalent -- avoid)_ | `var x = 5;` | Function-scoped (legacy) |

### Rules

```js
// const -- use by default
const name = "Alice";
// name = "Bob";  // TypeError: Assignment to constant variable

// let -- use when you need to reassign
let count = 0;
count = 1; // fine

// var -- never use (function-scoped, hoisted, causes subtle bugs)
```

### Important: `const` does NOT mean immutable

```js
const user = { name: "Alice" };
user.name = "Bob";    // This works! The reference is constant, not the contents.
user = { name: "C" }; // TypeError -- can't reassign the variable itself
```

This is like `readonly` in C# for reference types -- the reference can't change, but the object's properties can. If you want true immutability, you need `Object.freeze()` or immutability libraries.

### Rule of Thumb

Use `const` everywhere. Switch to `let` only when you genuinely need reassignment. Never use `var`.

---

## 2. Arrow Functions

### C# Comparison

Arrow functions are JavaScript's version of lambda expressions.

```csharp
// C# lambda
Func<int, int> double = x => x * 2;
Action<string> greet = name => Console.WriteLine($"Hello {name}");
```

```js
// JS arrow function
const double = (x) => x * 2;
const greet = (name) => console.log(`Hello ${name}`);
```

### Syntax Variations

```js
// Single parameter -- parentheses optional
const square = x => x * 2;

// Multiple parameters -- parentheses required
const add = (a, b) => a + b;

// No parameters -- empty parentheses required
const greet = () => "Hello!";

// Multi-line body -- needs braces AND explicit return
const process = (x) => {
  const doubled = x * 2;
  return doubled + 1;
};

// Returning an object literal -- wrap in parentheses (otherwise JS thinks { } is a code block)
const makeUser = (name) => ({ name: name, active: true });
```

### Arrow Functions vs `function` Keyword

```js
// Traditional function
function add(a, b) {
  return a + b;
}

// Arrow function
const add = (a, b) => a + b;
```

The key behavioral difference: arrow functions don't have their own `this`. They capture `this` from the enclosing scope (like a closure). Traditional functions have their own `this` that depends on how they're called. In React, this distinction rarely matters because you'll use arrow functions almost exclusively.

### When to Use Which

- **Arrow functions**: default choice for callbacks, component functions, event handlers
- **`function` keyword**: occasionally needed when you need `this` binding or hoisting (rare in React)

---

## 3. Template Literals

### C# Comparison

```csharp
// C# string interpolation
string message = $"Hello {name}, you have {count} items";
string multiLine = $"""
    Hello {name},
    Welcome!
    """;
```

```js
// JS template literals -- backticks, not quotes
const message = `Hello ${name}, you have ${count} items`;
const multiLine = `
  Hello ${name},
  Welcome!
`;
```

### Key Details

```js
// Any expression works inside ${}
const summary = `Total: ${items.length * price}`;
const greeting = `Hello ${firstName.toUpperCase()}`;
const conditional = `Status: ${isActive ? "Active" : "Inactive"}`;

// Regular strings use single or double quotes (convention: prefer double in JSX, single elsewhere)
const simple = 'no interpolation needed';
```

### Tagged Templates (Advanced -- Just Know They Exist)

```js
// Libraries like styled-components use this syntax
const Button = styled.button`
  background: ${props => props.primary ? "blue" : "gray"};
  color: white;
`;
```

---

## 4. Destructuring

Destructuring is syntactic sugar for pulling values out of objects and arrays. No direct C# equivalent existed until C# 12's limited support -- JS's version is more powerful and used **everywhere** in React.

### Object Destructuring

```js
const user = { name: "Alice", age: 30, email: "alice@example.com" };

// Without destructuring
const name = user.name;
const age = user.age;

// With destructuring -- pulls out properties by name
const { name, age } = user;

// Rename while destructuring
const { name: userName, age: userAge } = user;

// Default values (if property is undefined)
const { name, role = "user" } = user; // role will be "user"

// Nested destructuring
const response = { data: { user: { name: "Alice" } } };
const { data: { user: { name } } } = response;
```

### Array Destructuring

```js
const colors = ["red", "green", "blue"];

// Without destructuring
const first = colors[0];
const second = colors[1];

// With destructuring -- pulls by position
const [first, second] = colors;

// Skip elements
const [first, , third] = colors; // skip "green"

// With rest (grab the remainder)
const [first, ...rest] = colors; // rest = ["green", "blue"]
```

### In Function Parameters (Very Common in React)

```js
// Without destructuring
function greet(user) {
  return `Hello ${user.name}`;
}

// With destructuring in parameter
function greet({ name, age }) {
  return `Hello ${name}, age ${age}`;
}

// React components use this heavily
function UserCard({ name, email, avatar }) {
  // Instead of: function UserCard(props) { ... props.name, props.email ... }
}
```

### C# Analogy

```csharp
// C# deconstruction (limited)
var (x, y) = GetPoint(); // tuple deconstruction

// C# has no equivalent to JS object destructuring
// You'd write:
var name = user.Name;
var age = user.Age;
```

---

## 5. Spread and Rest (`...`)

The `...` operator does two things depending on context: **spread** (expand) or **rest** (collect).

### Spread -- Expanding

```js
// Spread arrays
const a = [1, 2, 3];
const b = [0, ...a, 4]; // [0, 1, 2, 3, 4]

// Spread objects (shallow copy + override)
const user = { name: "Alice", age: 30 };
const updated = { ...user, age: 31 }; // { name: "Alice", age: 31 }

// Copy an array (immutable update pattern -- critical in React)
const original = [1, 2, 3];
const copy = [...original];

// Merge objects
const defaults = { theme: "light", lang: "en" };
const prefs = { lang: "hu" };
const settings = { ...defaults, ...prefs }; // { theme: "light", lang: "hu" }
```

### Rest -- Collecting

```js
// Rest in function parameters (like C# params)
function sum(...numbers) {
  return numbers.reduce((total, n) => total + n, 0);
}
sum(1, 2, 3, 4); // 10

// Rest in destructuring
const { name, ...rest } = { name: "Alice", age: 30, email: "a@b.com" };
// name = "Alice", rest = { age: 30, email: "a@b.com" }

const [first, ...others] = [1, 2, 3, 4];
// first = 1, others = [2, 3, 4]
```

### C# Comparison

```csharp
// C# params keyword is like rest
void Sum(params int[] numbers) { ... }

// C# spread operator (C# 12 collection expressions)
int[] a = [1, 2, 3];
int[] b = [0, ..a, 4];

// C# with expressions for records (similar to object spread)
var updated = user with { Age = 31 };
```

### Why This Matters for React

React state must be updated immutably. You'll write patterns like this constantly:

```js
// Update one item in a state array
const updatedItems = items.map(item =>
  item.id === targetId ? { ...item, completed: true } : item
);

// Add an item to a state array
const withNewItem = [...items, newItem];

// Remove an item from a state array
const withoutItem = items.filter(item => item.id !== targetId);
```

---

## 6. Modules (`import` / `export`)

### C# Comparison

| C# | JavaScript |
|---|---|
| `namespace` + `using` | Files are modules; `import`/`export` |
| `public class Foo` | `export class Foo` or `export default class Foo` |
| `using MyLib;` | `import { Foo } from './myLib';` |

### Named Exports (Preferred)

```js
// math.js -- exporting
export const PI = 3.14;
export function add(a, b) { return a + b; }
export function multiply(a, b) { return a * b; }

// app.js -- importing
import { add, multiply } from './math.js';
import { add as sum } from './math.js'; // rename on import
import * as math from './math.js';       // namespace import: math.add(), math.PI
```

### Default Exports (One Per File)

```js
// UserCard.js
export default function UserCard({ name }) {
  return `<div>${name}</div>`;
}

// app.js -- can name it anything on import
import UserCard from './UserCard.js';
import MyCard from './UserCard.js'; // same thing, different local name
```

### Mixed

```js
// api.js
export default class ApiClient { ... }
export const BASE_URL = "https://api.example.com";

// app.js
import ApiClient, { BASE_URL } from './api.js';
```

### Convention in React

- **Components**: usually default export (one component per file)
- **Utilities, constants, types**: named exports
- **Index files** (`index.js`): re-export from a folder for cleaner imports

```js
// components/index.js
export { default as UserCard } from './UserCard';
export { default as BookList } from './BookList';

// somewhere else
import { UserCard, BookList } from './components';
```

---

## 7. Array Methods (LINQ Equivalents)

This is where your C#/LINQ experience gives you a massive head start. Same concepts, different names.

### The Big Six

| LINQ (C#) | JavaScript | Purpose |
|---|---|---|
| `.Select(x => ...)` | `.map(x => ...)` | Transform each element |
| `.Where(x => ...)` | `.filter(x => ...)` | Keep elements matching condition |
| `.FirstOrDefault(x => ...)` | `.find(x => ...)` | Find first match (or `undefined`) |
| `.Aggregate(seed, (acc, x) => ...)` | `.reduce((acc, x) => ..., seed)` | Reduce to single value |
| `.Any(x => ...)` | `.some(x => ...)` | At least one matches? |
| `.All(x => ...)` | `.every(x => ...)` | All match? |

### `.map()` -- Transform (like `Select`)

```js
const numbers = [1, 2, 3, 4];
const doubled = numbers.map(n => n * 2); // [2, 4, 6, 8]

const users = [{ name: "Alice" }, { name: "Bob" }];
const names = users.map(u => u.name); // ["Alice", "Bob"]
```

```csharp
// C# equivalent
var doubled = numbers.Select(n => n * 2).ToList();
```

### `.filter()` -- Keep Matches (like `Where`)

```js
const numbers = [1, 2, 3, 4, 5];
const evens = numbers.filter(n => n % 2 === 0); // [2, 4]

const users = [
  { name: "Alice", active: true },
  { name: "Bob", active: false },
];
const activeUsers = users.filter(u => u.active);
```

```csharp
// C# equivalent
var evens = numbers.Where(n => n % 2 == 0).ToList();
```

### `.find()` -- First Match (like `FirstOrDefault`)

```js
const users = [
  { id: 1, name: "Alice" },
  { id: 2, name: "Bob" },
];
const bob = users.find(u => u.id === 2); // { id: 2, name: "Bob" }
const nobody = users.find(u => u.id === 99); // undefined
```

```csharp
// C# equivalent
var bob = users.FirstOrDefault(u => u.Id == 2);
```

### `.reduce()` -- Accumulate (like `Aggregate`)

```js
const numbers = [1, 2, 3, 4];

// Sum
const sum = numbers.reduce((total, n) => total + n, 0); // 10

// Group by (common real-world use)
const items = [
  { category: "fruit", name: "apple" },
  { category: "veggie", name: "carrot" },
  { category: "fruit", name: "banana" },
];

const grouped = items.reduce((groups, item) => {
  const key = item.category;
  groups[key] = groups[key] || [];
  groups[key].push(item);
  return groups;
}, {});
// { fruit: [{...}, {...}], veggie: [{...}] }
```

```csharp
// C# equivalent
var sum = numbers.Aggregate(0, (total, n) => total + n);
var grouped = items.GroupBy(i => i.Category); // C# has a dedicated method
```

### `.some()` and `.every()` -- Test (like `Any` and `All`)

```js
const numbers = [1, 2, 3, 4, 5];

numbers.some(n => n > 3);  // true  -- at least one > 3
numbers.every(n => n > 0); // true  -- all > 0
numbers.every(n => n > 3); // false -- not all > 3
```

```csharp
// C# equivalents
numbers.Any(n => n > 3);
numbers.All(n => n > 0);
```

### Chaining (like LINQ Chaining)

```js
const result = users
  .filter(u => u.active)
  .map(u => u.name)
  .sort();
```

```csharp
// C# equivalent
var result = users
    .Where(u => u.Active)
    .Select(u => u.Name)
    .OrderBy(n => n)
    .ToList();
```

### Other Useful Array Methods

```js
// .includes() -- like .Contains()
[1, 2, 3].includes(2); // true

// .indexOf() -- like .IndexOf()
["a", "b", "c"].indexOf("b"); // 1

// .flat() -- like .SelectMany() for nested arrays
[[1, 2], [3, 4]].flat(); // [1, 2, 3, 4]

// .flatMap() -- map + flat in one step (like SelectMany with projection)
const sentences = ["hello world", "foo bar"];
sentences.flatMap(s => s.split(" ")); // ["hello", "world", "foo", "bar"]

// .findIndex() -- index of first match
[10, 20, 30].findIndex(n => n > 15); // 1

// .sort() -- MUTATES the original array! (unlike LINQ's OrderBy)
// To sort without mutating:
const sorted = [...numbers].sort((a, b) => a - b);

// .forEach() -- side effects only (no return value, unlike .map)
names.forEach(name => console.log(name));
```

### Key Difference from LINQ

LINQ is lazy (deferred execution). JavaScript array methods are **eager** -- they execute immediately and return a new array. No `.ToList()` needed, but also no free "only-iterate-once" optimization.

---

## 8. Closures

A closure is when a function "remembers" variables from its outer scope even after that scope has finished executing.

### Basic Example

```js
function createCounter() {
  let count = 0; // This variable is "closed over"
  return function () {
    count++;
    return count;
  };
}

const counter = createCounter();
counter(); // 1
counter(); // 2
counter(); // 3
// count is not accessible from outside, but the returned function still has access to it
```

### C# Comparison

C# has closures too -- you use them with lambdas:

```csharp
Func<int> CreateCounter()
{
    int count = 0; // captured variable
    return () => ++count;
}

var counter = CreateCounter();
counter(); // 1
counter(); // 2
```

### Practical Uses

```js
// 1. Data privacy (like private fields)
function createUser(name) {
  let loginCount = 0;
  return {
    getName: () => name,
    login: () => { loginCount++; },
    getLoginCount: () => loginCount,
  };
}

// 2. Function factories
function createMultiplier(factor) {
  return (number) => number * factor;
}
const double = createMultiplier(2);
const triple = createMultiplier(3);
double(5); // 10
triple(5); // 15

// 3. Event handlers (very common in React)
function setupButtons(buttons) {
  buttons.forEach((btn, index) => {
    btn.addEventListener("click", () => {
      console.log(`Button ${index} clicked`); // index is closed over
    });
  });
}
```

### The Classic Closure Trap

```js
// Bug: all timeouts print 3
for (var i = 0; i < 3; i++) {
  setTimeout(() => console.log(i), 100);
}
// Prints: 3, 3, 3 (because var is function-scoped, so there's only one i)

// Fix: use let (block-scoped, creates a new i for each iteration)
for (let i = 0; i < 3; i++) {
  setTimeout(() => console.log(i), 100);
}
// Prints: 0, 1, 2
```

This is another reason to never use `var`.

---

## 9. Higher-Order Functions

A higher-order function either **takes a function as an argument** or **returns a function**. You already use this concept constantly in C# with LINQ and delegates.

### C# Comparison

```csharp
// C# -- Func<T> is the concept, LINQ methods are higher-order functions
var evens = numbers.Where(n => n % 2 == 0); // Where takes a Func<int, bool>

// C# -- returning a function
Func<int, int> CreateAdder(int x) => y => x + y;
```

```js
// JS -- array methods are higher-order functions
const evens = numbers.filter(n => n % 2 === 0); // filter takes a function

// JS -- returning a function
const createAdder = (x) => (y) => x + y;
const add5 = createAdder(5);
add5(3); // 8
```

### Common Patterns

```js
// 1. Callbacks (function as argument)
function fetchData(url, onSuccess, onError) {
  // ...
  onSuccess(data);
}

// 2. Array methods (you already know these from section 7)
[1, 2, 3].map(n => n * 2);

// 3. Function composition
const compose = (f, g) => (x) => f(g(x));
const addOne = (x) => x + 1;
const double = (x) => x * 2;
const addOneThenDouble = compose(double, addOne);
addOneThenDouble(3); // 8

// 4. Decorators / wrappers
function withLogging(fn) {
  return (...args) => {
    console.log(`Calling with args:`, args);
    const result = fn(...args);
    console.log(`Result:`, result);
    return result;
  };
}
const loggedAdd = withLogging((a, b) => a + b);
loggedAdd(2, 3); // logs args, result, returns 5
```

### In React

React hooks like `useEffect`, `useCallback`, and `useMemo` are higher-order patterns. Event handlers are callbacks. You'll use higher-order functions constantly.

---

## 10. Callback Patterns

Before Promises and `async/await` (covered in Week 2), JavaScript used callbacks for asynchronous operations. Understanding callbacks is important because:
1. Many APIs still use them (event listeners, `setTimeout`, Node.js)
2. They're the foundation for understanding Promises

### Basic Callback

```js
function doSomethingAsync(callback) {
  setTimeout(() => {
    callback("done!");
  }, 1000);
}

doSomethingAsync((result) => {
  console.log(result); // "done!" after 1 second
});
```

### Error-First Callbacks (Node.js Convention)

```js
function readFile(path, callback) {
  // callback(error, data) -- error is first parameter
  if (/* something went wrong */) {
    callback(new Error("File not found"), null);
  } else {
    callback(null, fileContents);
  }
}

readFile("/data.json", (error, data) => {
  if (error) {
    console.error("Failed:", error.message);
    return;
  }
  console.log(data);
});
```

### Callback Hell (Why Promises Were Invented)

```js
// Nested callbacks become unreadable quickly
getUser(userId, (err, user) => {
  getOrders(user.id, (err, orders) => {
    getOrderDetails(orders[0].id, (err, details) => {
      // 3 levels deep and growing...
    });
  });
});
```

This is solved by Promises and `async/await` (Week 2). You'll rarely write raw callbacks in React, but you need to recognize the pattern since event handlers (`onClick`, `onChange`) are callbacks.

---

## 11. Short-Circuit Evaluation (`&&`, `??`, `?.`)

These operators are used **constantly** in React for conditional rendering and safe property access.

### `&&` -- Logical AND (Conditional Rendering)

```js
// Returns the first falsy value, or the last value if all are truthy
true && "hello";   // "hello"
false && "hello";  // false
0 && "hello";      // 0
"hi" && "hello";   // "hello"
```

```jsx
// React usage: conditionally render something
function Greeting({ user }) {
  return (
    <div>
      {user && <span>Welcome, {user.name}!</span>}
      {/* If user is truthy, render the span. If falsy, render nothing. */}
    </div>
  );
}
```

### `||` -- Logical OR (Default Values -- Legacy)

```js
// Returns the first truthy value
const name = userInput || "Anonymous";
// Problem: 0, "", and false are falsy, so they get replaced too
const count = userCount || 10; // If userCount is 0, this gives 10 (probably a bug!)
```

### `??` -- Nullish Coalescing (Better Default Values)

```js
// Returns the right side ONLY if the left is null or undefined (not 0, "", or false)
const name = userInput ?? "Anonymous";
const count = userCount ?? 10; // If userCount is 0, this correctly gives 0
```

```csharp
// C# -- identical behavior
var name = userInput ?? "Anonymous";
```

### `?.` -- Optional Chaining

```js
const user = { address: { city: "Budapest" } };

// Without optional chaining
const city = user && user.address && user.address.city;

// With optional chaining
const city = user?.address?.city; // "Budapest"
const zip = user?.address?.zip;   // undefined (no error)

// Works with methods too
const length = user?.getName?.(); // calls getName() if it exists

// Works with arrays
const first = users?.[0]?.name;
```

```csharp
// C# -- identical syntax and behavior
var city = user?.Address?.City;
```

### Combining Them (Very Common Pattern)

```js
// Get deeply nested value with a fallback
const city = user?.address?.city ?? "Unknown";
const displayName = user?.profile?.name ?? user?.email ?? "Anonymous";
```

### Falsy vs Nullish -- Know the Difference

```js
// These values are falsy (evaluate to false in boolean context):
false, 0, -0, 0n, "", null, undefined, NaN

// These values are nullish (only null and undefined):
null, undefined

// This matters for choosing || vs ??
const val1 = 0 || 10;  // 10 (0 is falsy)
const val2 = 0 ?? 10;  // 0  (0 is not nullish)
const val3 = "" || "default";  // "default" (empty string is falsy)
const val4 = "" ?? "default";  // "" (empty string is not nullish)
```

---

## 12. Key Differences from C# to Keep in Mind

### Type System

```js
// JS is dynamically typed -- no type annotations (TypeScript adds them back in Week 2)
let x = 5;       // number
x = "hello";     // valid in JS, would be a compile error in C#

// typeof operator
typeof 42;        // "number"
typeof "hello";   // "string"
typeof true;      // "boolean"
typeof undefined; // "undefined"
typeof null;      // "object" (infamous JS bug, been there since 1995)
typeof [];        // "object" (arrays are objects)
```

### Equality

```js
// == (loose equality) -- performs type coercion, AVOID
1 == "1";    // true (string coerced to number)
0 == false;  // true
null == undefined; // true

// === (strict equality) -- no coercion, ALWAYS USE THIS
1 === "1";   // false
0 === false; // false

// Objects/arrays are compared by reference, not value (same as C# reference types)
[1, 2] === [1, 2]; // false (different array instances)
const a = [1, 2];
const b = a;
a === b; // true (same reference)
```

### Truthiness

```js
// JS has "truthy" and "falsy" values -- everything is implicitly convertible to boolean
// Falsy: false, 0, -0, 0n, "", null, undefined, NaN
// Truthy: everything else (including [], {}, "0", "false")

if ([]) console.log("empty array is truthy!"); // This PRINTS
if ("") console.log("empty string is truthy"); // This does NOT print

// C# doesn't have this -- only bool works in if conditions
```

### `undefined` vs `null`

```js
// JS has two "nothing" values (C# only has null)
let x;           // undefined (declared but not assigned)
let y = null;    // null (explicitly set to nothing)

// Missing object properties are undefined
const obj = { a: 1 };
obj.b; // undefined (not an error, unlike C# which would be a compile error)

// Missing function arguments are undefined
function greet(name) {
  console.log(name); // undefined if called as greet()
}
```

### Objects Are Not Classes (by Default)

```js
// JS objects are like dictionaries/anonymous types
const user = {
  name: "Alice",
  age: 30,
  greet() {
    return `Hi, I'm ${this.name}`;
  },
};

// Properties can be added/removed dynamically
user.email = "alice@example.com"; // added
delete user.age;                   // removed

// JS does have classes too (syntactic sugar over prototypes)
class User {
  constructor(name) {
    this.name = name;
  }
  greet() {
    return `Hi, I'm ${this.name}`;
  }
}
```

### No Method Overloading

```js
// C# allows multiple methods with same name but different parameters
// JS does not -- the last function definition wins
function greet(name) { return `Hello ${name}`; }
function greet(first, last) { return `Hello ${first} ${last}`; }
// Only the second one exists now

// Instead, use default parameters and checking
function greet(first, last = "") {
  return last ? `Hello ${first} ${last}` : `Hello ${first}`;
}
```

---

## Exercises

1. **Destructuring practice**: Given `const book = { title: "DDD", author: { name: "Eric Evans", country: "US" }, year: 2003 }`, extract `title`, author's `name` (as `authorName`), and `year` in one destructuring statement.

2. **Array methods**: Given an array of books with `{ title, rating, genre }`, write a chain that: filters to books with rating > 4, maps to just the titles, and sorts alphabetically.

3. **Spread/immutable update**: Given `const state = { users: [{ id: 1, name: "Alice" }, { id: 2, name: "Bob" }], loading: false }`, write code to produce a new state where Bob's name is changed to "Robert" without mutating the original.

4. **Closure**: Write a `createGreeter(greeting)` function that returns a function accepting a name and returning `"${greeting}, ${name}!"`. Create `hello` and `hi` greeters from it.

5. **Reduce**: Use `.reduce()` to count the number of occurrences of each word in an array like `["apple", "banana", "apple", "cherry", "banana", "apple"]`.
