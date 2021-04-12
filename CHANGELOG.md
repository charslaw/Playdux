# CHANGELOG

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## Version 3.0.0

*Merged to master on 2021-04-12*

### ADDED

- Add `InitializeAction`, which handles initializing or reinitializing a `Store`.

### CHANGED

- Rename project to "Playdux".
- Updated to Unity 2021.2 for C#9 support.
  - Use `record` whenever possible as opposed to `class`.
  - Use `#nullable enable` in all files.

---

## Version 2.1.2

*Merged to master on 2020-03-01*

Adds the missing aresso_icon meta file as not having it causes error messages in projects that import the AReSSO package.

### FIXED

- Add aresso_icon.svg.meta.

---

## Version 2.1.1

*Merged to master on 2020-02-26*

### FIXED

- Update package.json version.
- Update package.json with UniRx dependency.

---

## Version 2.1.0

*Merged to master on 2020-02-26*

Add `StoreBehaviour`, a `MonoBehaviour` wrapper for a `Store`. Also extracts interfaces for `Store`.

### ADDED

- `StoreBehaviour`
- `StoreBehaviour` examples

### CHANGED

- Extract `IStore`, `IActionDispatcher`, and `IStateContainer` from `Store`.
- Simplify, improve the example `Copy` behavior for example classes.

### FIXED

- Date format in change log is wrong 🤦‍♂️

---

## Version 2.0.0

*Merged to master on 2020-02-25*

Moves `Store` and `IAction` to the `Store` namespace. Though small, this is a breaking change, thus v2.0.0.

### ADDED

- Added logo to readme.

### CHANGED

- Update readme with more detailed usage info.
- Modify change log formatting to make it more readable.
- Moved `Store` to `AReSSO.Store` namespace.
- Moved `IAction` to `AReSSO.Store` namespace.
- Moved `PropertyChange` to `AReSSO.CopyUtils` namespace.

---

## Version 1.0.0

*Merged to master on 2020-02-23*

This is the first functional version of AReSSO.

This version adds the `Store`. The `Store` presents the ability to `Dispatch` actions, get the current root state,
and get an observable version of the store that updates when the store changes.

### ADDED

- Add `Store`
- Add `IAction`
- Add tests for `Store`
- Add a few example state objects with documentation.
- Add `PropertyChange` utility to make writing `Copy` methods easier.

---

## Version 0.0.0

*Merged to master on 2020-02-20*

Initial version.
