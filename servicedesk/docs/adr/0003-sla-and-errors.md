# ADR 0003: Calendar SLA and Problem Details

**Decision:** Calculate deadlines from original creation in calendar hours and expose failure responses using HTTP Problem Details. Expected failures use 400 (invalid input/CSRF), 401 (not signed in), 403 (operation forbidden), 404 (ticket invisible or absent), or 409 (state conflict).

**Reason:** The rule is deterministic, small enough to explain and test, and avoids implying unsupported business-hour calendars. Standard problem responses give both clients one predictable error shape.

**Consequence:** There is no holiday calendar, escalation or notification flow. Client code must not infer ticket visibility from its own UI state.
