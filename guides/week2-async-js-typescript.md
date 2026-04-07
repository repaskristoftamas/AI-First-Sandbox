# Week 2 -- Async JavaScript + TypeScript Basics

Building on Week 1. First half covers async patterns (very similar to C#'s `async/await`), second half covers TypeScript (bringing back the type safety you're used to).

---

## Table of Contents

### Part 1: Async JavaScript
1. [Promises](#1-promises)
2. [`async` / `await`](#2-async--await)
3. [`fetch()` -- HTTP Client](#3-fetch--http-client)
4. [Error Handling (`catch`, `try-catch`)](#4-error-handling-catch-try-catch)
5. [`AbortController` -- Cancellation](#5-abortcontroller--cancellation)
6. [Reference vs Value Equality](#6-reference-vs-value-equality)
7. [Immutability Patterns](#7-immutability-patterns)

### Part 2: TypeScript Basics
8. [Why TypeScript](#8-why-typescript)
9. [Basic Types](#9-basic-types)
10. [Interfaces and Type Aliases](#10-interfaces-and-type-aliases)
11. [Union Types and Literal Types](#11-union-types-and-literal-types)
12. [Optional Properties and Parameters](#12-optional-properties-and-parameters)
13. [Generics](#13-generics)
14. [Type Narrowing](#14-type-narrowing)
15. [Utility Types](#15-utility-types)
16. [Discriminated Unions](#16-discriminated-unions)
17. [`as const`](#17-as-const)
18. [Strict Mode](#18-strict-mode)
19. [Key Differences from C# Generics and Types](#19-key-differences-from-c-generics-and-types)

---

## Part 1: Async JavaScript

---

## 1. Promises

A Promise represents a value that will be available in the future. It's conceptually identical to `Task<T>` in C#.

### C# Comparison

| C# | JavaScript |
|---|---|
| `Task<T>` | `Promise<T>` |
| `Task.FromResult(value)` | `Promise.resolve(value)` |
| `Task.FromException(ex)` | `Promise.reject(error)` |
| `Task.WhenAll(...)` | `Promise.all(...)` |
| `Task.WhenAny(...)` | `Promise.race(...)` |

### Promise States

```
Pending  -->  Fulfilled (resolved with a value)
         -->  Rejected (rejected with an error)
```

### Creating a Promise

```js
const promise = new Promise((resolve, reject) => {
  // Do some async work
  setTimeout(() => {
    const success = true;
    if (success) {
      resolve("Data loaded!");    // Fulfill the promise
    } else {
      reject(new Error("Failed")); // Reject the promise
    }
  }, 1000);
});
```

### Consuming a Promise (`.then` / `.catch` / `.finally`)

```js
promise
  .then(result => {
    console.log(result); // "Data loaded!"
    return result.toUpperCase(); // can chain by returning a value
  })
  .then(upper => {
    console.log(upper); // "DATA LOADED!"
  })
  .catch(error => {
    console.error("Error:", error.message);
  })
  .finally(() => {
    console.log("Always runs, like C#'s finally");
  });
```

### Promise Combinators

```js
// Promise.all -- wait for ALL to complete (like Task.WhenAll)
const [users, posts] = await Promise.all([
  fetchUsers(),
  fetchPosts(),
]);
// If ANY promise rejects, the whole thing rejects

// Promise.allSettled -- wait for all, even if some fail
const results = await Promise.allSettled([
  fetchUsers(),
  fetchPosts(),
]);
// results = [
//   { status: "fulfilled", value: [...] },
//   { status: "rejected", reason: Error }
// ]

// Promise.race -- first to settle wins (like Task.WhenAny)
const fastest = await Promise.race([
  fetchFromServerA(),
  fetchFromServerB(),
]);

// Promise.any -- first to FULFILL wins (ignores rejections)
const first = await Promise.any([
  fetchFromServerA(),
  fetchFromServerB(),
]);
```

---

## 2. `async` / `await`

Syntactic sugar over Promises, just like C#'s `async/await` is syntactic sugar over Tasks. The behavior is almost identical.

### C# Comparison

```csharp
// C#
public async Task<User> GetUserAsync(int id)
{
    var response = await _httpClient.GetAsync($"/users/{id}");
    var user = await response.Content.ReadFromJsonAsync<User>();
    return user;
}
```

```js
// JavaScript
async function getUser(id) {
  const response = await fetch(`/users/${id}`);
  const user = await response.json();
  return user;
}
```

### Key Rules

```js
// 1. async functions always return a Promise
async function greet() {
  return "hello";
}
// Same as: function greet() { return Promise.resolve("hello"); }

// 2. await can only be used inside async functions (or at top-level in modules)
async function loadData() {
  const data = await fetchData(); // pauses here until fetchData resolves
  console.log(data);
}

// 3. await unwraps the Promise value
const response = await fetch(url); // response is a Response object, not a Promise

// 4. Sequential vs parallel
// Sequential (one after the other):
const users = await fetchUsers();
const posts = await fetchPosts(); // waits for fetchUsers to finish first

// Parallel (both start at the same time):
const [users, posts] = await Promise.all([
  fetchUsers(),
  fetchPosts(),
]);
```

### Common Mistake: Forgetting `await`

```js
// Bug: response is a Promise, not the actual response!
async function getData() {
  const response = fetch("/api/data"); // missing await!
  const data = response.json(); // TypeError: response.json is not a function
}

// Fix:
async function getData() {
  const response = await fetch("/api/data");
  const data = await response.json();
  return data;
}
```

---

## 3. `fetch()` -- HTTP Client

`fetch()` is JavaScript's built-in HTTP client. It's the equivalent of `HttpClient` in C#.

### Basic GET

```js
const response = await fetch("https://api.example.com/users");
const users = await response.json(); // parse JSON body
```

```csharp
// C# equivalent
var response = await _httpClient.GetAsync("https://api.example.com/users");
var users = await response.Content.ReadFromJsonAsync<List<User>>();
```

### Important: `fetch` Doesn't Throw on HTTP Errors

```js
// fetch only rejects on NETWORK errors (no internet, DNS failure, etc.)
// HTTP 404, 500, etc. are NOT rejections -- you must check manually

const response = await fetch("/api/users/999");
console.log(response.ok);     // false (status is 404)
console.log(response.status); // 404

// Common pattern: check response.ok
if (!response.ok) {
  throw new Error(`HTTP ${response.status}: ${response.statusText}`);
}
const data = await response.json();
```

This is a major gotcha coming from C#, where `HttpClient` has `EnsureSuccessStatusCode()`.

### POST / PUT / DELETE

```js
// POST with JSON body
const response = await fetch("/api/users", {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
  },
  body: JSON.stringify({ name: "Alice", email: "alice@example.com" }),
});

// PUT
await fetch(`/api/users/${id}`, {
  method: "PUT",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify(updatedUser),
});

// DELETE
await fetch(`/api/users/${id}`, {
  method: "DELETE",
});
```

```csharp
// C# equivalent
await _httpClient.PostAsJsonAsync("/api/users", new { Name = "Alice", Email = "alice@example.com" });
```

### Reading Different Response Types

```js
const json = await response.json();   // Parse as JSON
const text = await response.text();   // Raw text
const blob = await response.blob();   // Binary data (images, files)
const form = await response.formData(); // Form data
```

### Headers and Authentication

```js
const response = await fetch("/api/protected", {
  headers: {
    "Authorization": `Bearer ${token}`,
    "Content-Type": "application/json",
    "Accept": "application/json",
  },
});
```

---

## 4. Error Handling (`catch`, `try-catch`)

### With `async/await` (Preferred)

```js
async function loadUser(id) {
  try {
    const response = await fetch(`/api/users/${id}`);
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    console.error("Failed to load user:", error.message);
    throw error; // re-throw if you want callers to handle it
  }
}
```

```csharp
// C# equivalent
async Task<User> LoadUserAsync(int id)
{
    try
    {
        var response = await _httpClient.GetAsync($"/api/users/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>();
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Failed to load user");
        throw;
    }
}
```

### With Promise Chains

```js
fetch(`/api/users/${id}`)
  .then(response => {
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return response.json();
  })
  .then(user => {
    console.log(user);
  })
  .catch(error => {
    console.error("Failed:", error.message);
  });
```

### Error Types

```js
// JavaScript has fewer built-in error types than C#
try {
  // ...
} catch (error) {
  if (error instanceof TypeError) {
    // Type-related errors (calling non-function, accessing property of undefined)
  } else if (error instanceof RangeError) {
    // Number out of range
  } else if (error instanceof SyntaxError) {
    // Malformed JSON, etc.
  }
  // You can also create custom errors
}

// Custom error classes
class ApiError extends Error {
  constructor(status, message) {
    super(message);
    this.status = status;
    this.name = "ApiError";
  }
}
throw new ApiError(404, "User not found");
```

### Handling Multiple Async Operations

```js
async function loadDashboard() {
  try {
    const [users, posts, stats] = await Promise.all([
      fetchUsers(),
      fetchPosts(),
      fetchStats(),
    ]);
    return { users, posts, stats };
  } catch (error) {
    // If ANY of the three fails, we end up here
    console.error("Dashboard load failed:", error);
  }
}

// If you want to handle partial failures:
async function loadDashboard() {
  const results = await Promise.allSettled([
    fetchUsers(),
    fetchPosts(),
    fetchStats(),
  ]);

  const users = results[0].status === "fulfilled" ? results[0].value : [];
  const posts = results[1].status === "fulfilled" ? results[1].value : [];
  const stats = results[2].status === "fulfilled" ? results[2].value : null;

  return { users, posts, stats };
}
```

---

## 5. `AbortController` -- Cancellation

`AbortController` is JavaScript's version of `CancellationToken` from C#.

### C# Comparison

```csharp
// C#
var cts = new CancellationTokenSource();
await _httpClient.GetAsync(url, cts.Token);
cts.Cancel();
```

```js
// JavaScript
const controller = new AbortController();
await fetch(url, { signal: controller.signal });
controller.abort();
```

### Practical Example: Cancel on Component Unmount

```js
// This pattern is critical in React to prevent "state update on unmounted component" warnings
async function fetchWithTimeout(url, timeoutMs = 5000) {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const response = await fetch(url, { signal: controller.signal });
    clearTimeout(timeoutId);
    return await response.json();
  } catch (error) {
    if (error.name === "AbortError") {
      console.log("Request was cancelled");
    }
    throw error;
  }
}
```

### In React (Preview -- You'll Use This in Week 3)

```js
useEffect(() => {
  const controller = new AbortController();

  async function loadData() {
    try {
      const response = await fetch("/api/data", { signal: controller.signal });
      const data = await response.json();
      setData(data);
    } catch (error) {
      if (error.name !== "AbortError") {
        setError(error);
      }
    }
  }

  loadData();

  // Cleanup function: called when component unmounts
  return () => controller.abort();
}, []);
```

---

## 6. Reference vs Value Equality

This works the same as C#, but it's critical to understand for React's rendering model.

### Primitives -- Compared by Value

```js
const a = 5;
const b = 5;
a === b; // true -- same value

const x = "hello";
const y = "hello";
x === y; // true -- same value
```

### Objects/Arrays -- Compared by Reference

```js
const a = { name: "Alice" };
const b = { name: "Alice" };
a === b; // false! Different objects in memory (same as C# reference types)

const c = a;
a === c; // true -- same reference

// Arrays too
[1, 2, 3] === [1, 2, 3]; // false
```

### Why This Matters for React

React uses reference equality (`===`) to decide if state has changed and whether to re-render. If you mutate an object in place, React won't detect the change:

```js
// WRONG -- React won't re-render because the reference didn't change
const handleClick = () => {
  user.name = "Bob";    // mutating the existing object
  setUser(user);        // same reference, React thinks nothing changed
};

// CORRECT -- Create a new object so React sees a new reference
const handleClick = () => {
  setUser({ ...user, name: "Bob" }); // new object = new reference = re-render
};
```

### Comparing Objects by Value

```js
// No built-in deep equality in JS (C# has records with value equality)
// Options:
JSON.stringify(a) === JSON.stringify(b); // hacky, order-dependent, slow
// Or use a library like lodash: _.isEqual(a, b)
```

---

## 7. Immutability Patterns

React requires immutable state updates. These patterns come from Week 1's spread operator but are collected here for reference.

### Updating Object Properties

```js
const user = { name: "Alice", age: 30, email: "a@b.com" };

// Update one property
const updated = { ...user, age: 31 };

// Update nested property
const state = { user: { name: "Alice", address: { city: "Budapest" } } };
const newState = {
  ...state,
  user: {
    ...state.user,
    address: {
      ...state.user.address,
      city: "Vienna",
    },
  },
};
// Yes, deep updates are verbose. Libraries like Immer solve this (Week 4+).
```

### Updating Arrays

```js
const items = [{ id: 1, text: "a" }, { id: 2, text: "b" }, { id: 3, text: "c" }];

// Add item
const added = [...items, { id: 4, text: "d" }];

// Remove item
const removed = items.filter(item => item.id !== 2);

// Update one item
const updated = items.map(item =>
  item.id === 2 ? { ...item, text: "updated" } : item
);

// Insert at position
const inserted = [...items.slice(0, 1), { id: 4, text: "new" }, ...items.slice(1)];
```

### C# Comparison

```csharp
// C# records with `with` expressions (similar concept)
var updated = user with { Age = 31 };

// C# immutable collections
var newList = list.Add(item);
var removed = list.Remove(item);
```

### Methods That Mutate vs Return New Arrays

```js
// MUTATING (avoid in React state):
array.push(item);      // adds to end
array.pop();           // removes from end
array.splice(i, 1);    // removes at index
array.sort();          // sorts in place
array.reverse();       // reverses in place

// NON-MUTATING (safe for React state):
[...array, item];                      // add to end
array.slice(0, -1);                    // remove last
array.filter((_, idx) => idx !== i);   // remove at index
[...array].sort();                     // sort a copy
[...array].reverse();                  // reverse a copy
array.map(x => ...);                   // transform
array.filter(x => ...);               // filter
array.concat(otherArray);             // combine
```

---

## Part 2: TypeScript Basics

---

## 8. Why TypeScript

TypeScript adds static types to JavaScript. Coming from C#, you'll feel at home -- it brings back compile-time type safety, IntelliSense, and refactoring support.

```
TypeScript is to JavaScript
  what C# is to a hypothetical "C# without type annotations"
```

- TypeScript is a **superset** of JavaScript -- all valid JS is valid TS
- It compiles to JavaScript (the types are erased at runtime, unlike C#)
- It's the industry standard for React projects

### Setup (For Reference -- You'll Use Vite in Week 5)

```bash
npm install -D typescript
npx tsc --init  # creates tsconfig.json
```

Files use `.ts` extension (or `.tsx` for files containing JSX/React).

---

## 9. Basic Types

### C# to TypeScript Mapping

| C# | TypeScript | Notes |
|---|---|---|
| `int`, `double`, `decimal` | `number` | One type for all numbers |
| `string` | `string` | Same |
| `bool` | `boolean` | Same concept |
| `string[]` | `string[]` or `Array<string>` | Same |
| `object` | `object` | Rarely used directly |
| `dynamic` | `any` | Avoid -- defeats the purpose |
| `void` | `void` | Same |
| `null` | `null` | Same |
| _(no equivalent)_ | `undefined` | JS-specific "no value" |
| `(int, string)` | `[number, string]` | Tuple types |

### Syntax

```ts
// Variable types (usually inferred, like C#'s var)
const name: string = "Alice";
const age: number = 30;
const active: boolean = true;

// Type inference -- prefer this when the type is obvious
const name = "Alice"; // TS infers string
const age = 30;       // TS infers number

// Arrays
const numbers: number[] = [1, 2, 3];
const names: string[] = ["Alice", "Bob"];

// Tuples (fixed-length arrays with specific types per position)
const pair: [string, number] = ["Alice", 30];

// Function types
function add(a: number, b: number): number {
  return a + b;
}

// Arrow function types
const add = (a: number, b: number): number => a + b;

// Return type usually inferred
const add = (a: number, b: number) => a + b; // TS infers number return
```

### Special Types

```ts
// any -- opts out of type checking (avoid!)
let x: any = 5;
x = "hello"; // no error, defeats the purpose of TS

// unknown -- type-safe version of any (must narrow before use)
let x: unknown = 5;
// x.toFixed(); // Error! Must narrow first
if (typeof x === "number") {
  x.toFixed(); // OK after narrowing
}

// never -- function never returns (throws or infinite loop)
function throwError(msg: string): never {
  throw new Error(msg);
}

// void -- function returns nothing
function log(msg: string): void {
  console.log(msg);
}
```

---

## 10. Interfaces and Type Aliases

TypeScript has two ways to define object shapes: `interface` and `type`. Both work similarly.

### Interface (Like C# Interfaces, but for Shape)

```ts
interface User {
  id: number;
  name: string;
  email: string;
}

const user: User = {
  id: 1,
  name: "Alice",
  email: "alice@example.com",
};
```

```csharp
// C# record equivalent (for data shapes)
public record User(int Id, string Name, string Email);
```

### Type Alias

```ts
type User = {
  id: number;
  name: string;
  email: string;
};

// Can also alias primitives and unions (interfaces can't)
type ID = number | string;
type Status = "active" | "inactive" | "pending";
```

### Interface vs Type -- When to Use Which

| Feature | `interface` | `type` |
|---|---|---|
| Object shapes | Yes | Yes |
| Extend/inherit | `extends` | `&` (intersection) |
| Union types | No | Yes |
| Primitive aliases | No | Yes |
| Declaration merging | Yes (can add props later) | No |
| Convention | Objects, class contracts | Unions, primitives, computed types |

```ts
// Extending interfaces
interface Animal {
  name: string;
}
interface Dog extends Animal {
  breed: string;
}

// Intersection types (like extending, but for type aliases)
type Animal = { name: string };
type Dog = Animal & { breed: string };
```

### Key Difference from C# Interfaces

In C#, interfaces define contracts (methods to implement). In TypeScript, interfaces describe **shapes** -- they're structural, not nominal. If an object has the right properties, it satisfies the interface, regardless of whether it explicitly "implements" it.

```ts
interface Printable {
  toString(): string;
}

const obj = { toString: () => "hello", extra: 42 };
const p: Printable = obj; // Works! obj has the right shape (structural typing)
```

```csharp
// C# -- this would NOT work without explicit implementation
// class MyClass : IPrintable { ... }
```

---

## 11. Union Types and Literal Types

Union types are one of TypeScript's most powerful features. C# doesn't have a direct equivalent (until discriminated unions in future C# versions).

### Union Types

```ts
// A value that can be one of several types
type StringOrNumber = string | number;

let id: StringOrNumber;
id = 123;      // OK
id = "abc-123"; // OK
// id = true;  // Error

// Very common for API responses
type ApiResult = User | Error;

// Function that accepts multiple types
function formatId(id: string | number): string {
  if (typeof id === "string") {
    return id.toUpperCase();
  }
  return id.toString().padStart(5, "0");
}
```

### Literal Types

```ts
// A type that can only be one specific value
type Direction = "north" | "south" | "east" | "west";

let dir: Direction = "north"; // OK
// dir = "up"; // Error: "up" is not assignable to Direction

// Like enums but more flexible
type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";
type Status = "pending" | "active" | "inactive";
type DiceRoll = 1 | 2 | 3 | 4 | 5 | 6;
```

```csharp
// Closest C# equivalent is an enum
public enum Direction { North, South, East, West }
// But TS literal types are more flexible -- they can mix types, use string values directly
```

### Narrowing with Unions (See Section 14 for More)

```ts
function print(value: string | number) {
  // TypeScript error: can't call .toUpperCase() on string | number
  // Must narrow first:
  if (typeof value === "string") {
    console.log(value.toUpperCase()); // TS knows it's a string here
  } else {
    console.log(value.toFixed(2)); // TS knows it's a number here
  }
}
```

---

## 12. Optional Properties and Parameters

### Optional Properties

```ts
interface User {
  id: number;
  name: string;
  email?: string; // optional -- can be string | undefined
}

const user1: User = { id: 1, name: "Alice" }; // OK, email is optional
const user2: User = { id: 2, name: "Bob", email: "bob@test.com" }; // also OK
```

```csharp
// C# equivalent
public record User(int Id, string Name, string? Email = null);
```

### Optional Parameters

```ts
function greet(name: string, greeting?: string): string {
  return `${greeting ?? "Hello"}, ${name}!`;
}

greet("Alice");           // "Hello, Alice!"
greet("Alice", "Hi");     // "Hi, Alice!"
```

### Default Parameters

```ts
function greet(name: string, greeting: string = "Hello"): string {
  return `${greeting}, ${name}!`;
}
// greeting is inferred as string, not string | undefined
```

### Readonly Properties

```ts
interface User {
  readonly id: number;  // like C#'s init-only
  name: string;
}

const user: User = { id: 1, name: "Alice" };
// user.id = 2; // Error: Cannot assign to 'id' because it is a read-only property
```

---

## 13. Generics

TypeScript generics work almost identically to C# generics, with minor syntax differences.

### Basic Generics

```ts
// Generic function
function identity<T>(value: T): T {
  return value;
}
identity<string>("hello"); // explicit
identity(42);              // inferred as identity<number>

// Generic interface
interface ApiResponse<T> {
  data: T;
  status: number;
  message: string;
}

const response: ApiResponse<User[]> = {
  data: [{ id: 1, name: "Alice" }],
  status: 200,
  message: "OK",
};
```

```csharp
// C# equivalent -- almost identical
T Identity<T>(T value) => value;

public record ApiResponse<T>(T Data, int Status, string Message);
```

### Generic Constraints

```ts
// Constrain T to types that have a length property
function logLength<T extends { length: number }>(value: T): void {
  console.log(value.length);
}
logLength("hello");  // OK, string has length
logLength([1, 2]);   // OK, array has length
// logLength(42);    // Error, number has no length

// Constrain to specific interface
interface HasId {
  id: number;
}
function findById<T extends HasId>(items: T[], id: number): T | undefined {
  return items.find(item => item.id === id);
}
```

```csharp
// C# equivalent
void LogLength<T>(T value) where T : IHasLength { ... }
T? FindById<T>(List<T> items, int id) where T : IHasId { ... }
```

### Multiple Type Parameters

```ts
function pair<K, V>(key: K, value: V): [K, V] {
  return [key, value];
}
const p = pair("name", 42); // [string, number]

// With constraints
function merge<T extends object, U extends object>(a: T, b: U): T & U {
  return { ...a, ...b };
}
```

### Generic Defaults

```ts
interface PaginatedResult<T = unknown> {
  items: T[];
  total: number;
  page: number;
}

const result: PaginatedResult = { items: [], total: 0, page: 1 }; // T defaults to unknown
const users: PaginatedResult<User> = { items: [], total: 0, page: 1 }; // T is User
```

---

## 14. Type Narrowing

Type narrowing is how TypeScript narrows a broad type to a specific one within a code block. Similar to C#'s pattern matching.

### `typeof` Guards

```ts
function format(value: string | number): string {
  if (typeof value === "string") {
    return value.toUpperCase(); // TS knows: string
  }
  return value.toFixed(2); // TS knows: number
}
```

### `instanceof` Guards

```ts
class Dog { bark() { return "woof"; } }
class Cat { meow() { return "meow"; } }

function speak(animal: Dog | Cat): string {
  if (animal instanceof Dog) {
    return animal.bark(); // TS knows: Dog
  }
  return animal.meow(); // TS knows: Cat
}
```

### `in` Operator

```ts
interface Bird { fly(): void; }
interface Fish { swim(): void; }

function move(animal: Bird | Fish) {
  if ("fly" in animal) {
    animal.fly(); // TS knows: Bird
  } else {
    animal.swim(); // TS knows: Fish
  }
}
```

### Truthiness Narrowing

```ts
function greet(name: string | null | undefined) {
  if (name) {
    console.log(name.toUpperCase()); // TS knows: string (truthy)
  }
}
```

### Custom Type Guards (Type Predicates)

```ts
interface User { type: "user"; name: string; }
interface Admin { type: "admin"; name: string; permissions: string[]; }

// The return type "animal is Fish" is a type predicate
function isAdmin(person: User | Admin): person is Admin {
  return person.type === "admin";
}

function showPermissions(person: User | Admin) {
  if (isAdmin(person)) {
    console.log(person.permissions); // TS knows: Admin
  }
}
```

```csharp
// C# pattern matching equivalent
if (person is Admin admin)
{
    Console.WriteLine(admin.Permissions);
}
```

---

## 15. Utility Types

TypeScript has built-in utility types that transform existing types. These are like compiler-generated type modifications.

### `Partial<T>` -- All Properties Optional

```ts
interface User {
  id: number;
  name: string;
  email: string;
}

type PartialUser = Partial<User>;
// Equivalent to:
// { id?: number; name?: string; email?: string; }

// Common use: update functions
function updateUser(id: number, updates: Partial<User>): User {
  // Only pass the fields you want to change
}
updateUser(1, { name: "Bob" }); // only updating name
```

### `Required<T>` -- All Properties Required

```ts
type RequiredUser = Required<PartialUser>;
// All optional properties become required
```

### `Pick<T, Keys>` -- Select Specific Properties

```ts
type UserPreview = Pick<User, "id" | "name">;
// { id: number; name: string; }
```

```csharp
// No direct C# equivalent -- you'd create a new record/class
public record UserPreview(int Id, string Name);
```

### `Omit<T, Keys>` -- Remove Specific Properties

```ts
type UserWithoutEmail = Omit<User, "email">;
// { id: number; name: string; }

// Common: create input types from entity types
type CreateUserInput = Omit<User, "id">; // id is auto-generated
```

### `Record<Keys, Value>` -- Dictionary Type

```ts
type UserMap = Record<string, User>;
// { [key: string]: User }

// Useful for lookup tables
const usersById: Record<number, User> = {
  1: { id: 1, name: "Alice", email: "a@b.com" },
  2: { id: 2, name: "Bob", email: "b@c.com" },
};

// With literal union keys
type Roles = "admin" | "editor" | "viewer";
type RolePermissions = Record<Roles, string[]>;
```

```csharp
// C# equivalent
Dictionary<string, User> usersById = new();
```

### `Readonly<T>` -- All Properties Readonly

```ts
type ReadonlyUser = Readonly<User>;
const user: ReadonlyUser = { id: 1, name: "Alice", email: "a@b.com" };
// user.name = "Bob"; // Error
```

### `ReturnType<T>` -- Extract Function Return Type

```ts
function getUser() {
  return { id: 1, name: "Alice" };
}

type UserResult = ReturnType<typeof getUser>;
// { id: number; name: string; }
```

### `NonNullable<T>` -- Remove null and undefined

```ts
type MaybeString = string | null | undefined;
type DefiniteString = NonNullable<MaybeString>; // string
```

### Combining Utility Types

```ts
// Create an update input: all fields optional except id
type UpdateUserInput = Partial<Omit<User, "id">> & Pick<User, "id">;
// { id: number; name?: string; email?: string; }
```

---

## 16. Discriminated Unions

A pattern where each type in a union has a common literal property (the "discriminant") that TypeScript can use to narrow the type. This is the TypeScript equivalent of a sealed interface hierarchy or tagged union.

### Basic Example

```ts
type Shape =
  | { kind: "circle"; radius: number }
  | { kind: "rectangle"; width: number; height: number }
  | { kind: "triangle"; base: number; height: number };

function area(shape: Shape): number {
  switch (shape.kind) {
    case "circle":
      return Math.PI * shape.radius ** 2;
    case "rectangle":
      return shape.width * shape.height;
    case "triangle":
      return 0.5 * shape.base * shape.height;
  }
}
```

```csharp
// C# equivalent with pattern matching
public abstract record Shape;
public record Circle(double Radius) : Shape;
public record Rectangle(double Width, double Height) : Shape;

double Area(Shape shape) => shape switch
{
    Circle c => Math.PI * c.Radius * c.Radius,
    Rectangle r => r.Width * r.Height,
    _ => throw new ArgumentException()
};
```

### Real-World Use: API Responses

```ts
type ApiResult<T> =
  | { status: "success"; data: T }
  | { status: "error"; error: string }
  | { status: "loading" };

function handleResult(result: ApiResult<User[]>) {
  switch (result.status) {
    case "loading":
      showSpinner();
      break;
    case "error":
      showError(result.error); // TS knows error exists
      break;
    case "success":
      showUsers(result.data); // TS knows data exists
      break;
  }
}
```

### Exhaustiveness Checking

```ts
// TypeScript can warn you if you forget a case
function area(shape: Shape): number {
  switch (shape.kind) {
    case "circle":
      return Math.PI * shape.radius ** 2;
    case "rectangle":
      return shape.width * shape.height;
    // Forgot "triangle"!
    default:
      const _exhaustive: never = shape; // Error! triangle is not assignable to never
      return _exhaustive;
  }
}
```

This is like the C# `_ => throw new ArgumentException()` pattern but caught at compile time.

---

## 17. `as const`

`as const` makes TypeScript infer the narrowest possible type -- literal types instead of general types, and readonly throughout.

### Without `as const`

```ts
const config = {
  endpoint: "/api/users",
  method: "GET",
};
// Type: { endpoint: string; method: string; }
// method is just "string", not specifically "GET"
```

### With `as const`

```ts
const config = {
  endpoint: "/api/users",
  method: "GET",
} as const;
// Type: { readonly endpoint: "/api/users"; readonly method: "GET"; }
// method is specifically "GET", and the whole object is readonly
```

### Common Uses

```ts
// 1. Define constant arrays that preserve literal types
const ROLES = ["admin", "editor", "viewer"] as const;
type Role = (typeof ROLES)[number]; // "admin" | "editor" | "viewer"
// Without as const, Role would just be string

// 2. Config objects
const API_ROUTES = {
  users: "/api/users",
  books: "/api/books",
  authors: "/api/authors",
} as const;

// 3. Action types (for reducers/state management)
const ACTIONS = {
  ADD_USER: "ADD_USER",
  REMOVE_USER: "REMOVE_USER",
  UPDATE_USER: "UPDATE_USER",
} as const;
```

### C# Analogy

```csharp
// Similar to using const or static readonly with specific types
public static class Roles
{
    public const string Admin = "admin";
    public const string Editor = "editor";
}
```

---

## 18. Strict Mode

TypeScript's strict mode enables all strict type checking options. **Always use it.** It's like C#'s nullable reference types -- catches bugs at compile time.

### What Strict Mode Enables

```json
// tsconfig.json
{
  "compilerOptions": {
    "strict": true
    // This is shorthand for ALL of these:
    // "noImplicitAny": true,         -- error on implicit any types
    // "strictNullChecks": true,       -- null/undefined are separate types
    // "strictFunctionTypes": true,    -- stricter function type checking
    // "strictBindCallApply": true,    -- stricter bind/call/apply
    // "strictPropertyInitialization": true,  -- class props must be initialized
    // "noImplicitThis": true,         -- error on implicit any for this
    // "alwaysStrict": true,           -- emit "use strict"
    // "useUnknownInCatchVariables": true  -- catch variable is unknown, not any
  }
}
```

### Key Behaviors

```ts
// noImplicitAny -- must annotate when type can't be inferred
function greet(name) { }  // Error: Parameter 'name' implicitly has 'any' type
function greet(name: string) { }  // OK

// strictNullChecks -- null/undefined must be handled explicitly
function getLength(str: string | null): number {
  // return str.length; // Error: Object is possibly null
  return str?.length ?? 0; // OK
}

// useUnknownInCatchVariables
try { ... } catch (error) {
  // error is unknown, not any
  if (error instanceof Error) {
    console.log(error.message); // must narrow first
  }
}
```

---

## 19. Key Differences from C# Generics and Types

### Structural vs Nominal Typing

```ts
// TypeScript: structural typing (shape matters, name doesn't)
interface Point { x: number; y: number; }
interface Coordinate { x: number; y: number; }

const p: Point = { x: 1, y: 2 };
const c: Coordinate = p; // Works! Same shape = compatible

// C#: nominal typing (name matters)
// Point and Coordinate would be incompatible even with same properties
```

### Type Erasure

```ts
// TypeScript types are erased at runtime -- they don't exist in the compiled JS
// You CAN'T do this:
function isUser<T>(value: any): value is T {
  return value instanceof T; // Error! T doesn't exist at runtime
}

// C# CAN do this because types exist at runtime (reified generics)
// bool IsType<T>(object value) => value is T;
```

### Union Types (No C# Equivalent)

```ts
// TypeScript can express "this OR that" directly
type Result = Success | Failure;
type ID = string | number;

// C# requires inheritance/interfaces or OneOf library
```

### Index Signatures

```ts
// TypeScript can type dynamic property keys
interface StringMap {
  [key: string]: number;
}

const scores: StringMap = {
  alice: 95,
  bob: 87,
};
```

### Mapped Types (Advanced)

```ts
// Transform all properties of a type
type Optional<T> = {
  [K in keyof T]?: T[K];
};

// Same as Partial<T>, but you can build custom transformations
type Nullable<T> = {
  [K in keyof T]: T[K] | null;
};
```

---

## Exercises

### Async Exercises

1. **Basic fetch**: Write an async function `getUser(id: number)` that fetches from `https://jsonplaceholder.typicode.com/users/{id}`, checks `response.ok`, and returns the parsed JSON. Handle errors with try-catch.

2. **Parallel fetch**: Write a function that fetches both a user and their posts (from `/users/{id}` and `/posts?userId={id}`) in parallel using `Promise.all`, and returns `{ user, posts }`.

3. **Timeout**: Add AbortController-based timeout (3 seconds) to the `getUser` function.

4. **Immutable update**: Given a state object `{ users: User[], selectedId: number | null }`, write functions to: add a user, remove a user by id, and update a user's name -- all returning new state objects without mutation.

### TypeScript Exercises

5. **Type a function**: Write a typed `groupBy<T>(items: T[], key: keyof T): Record<string, T[]>` function.

6. **Discriminated union**: Define a `Result<T>` type with `success` and `error` variants. Write a function that accepts `Result<User>` and handles both cases.

7. **Utility types**: Given a `User` interface, derive: `CreateUserInput` (without id), `UpdateUserInput` (all optional except id), and `UserPreview` (only id and name).

8. **Rewrite**: Take 2-3 of your Week 1 JavaScript exercises and rewrite them in TypeScript with strict mode and zero `any` usage.
