# Domain rules

Requesters create and see only their own tickets. Technicians see and work only tickets assigned to them. Coordinators see all tickets and may assign technicians and change priorities. The API returns 404 for a ticket outside a user's visible set, 403 for a forbidden operation on a visible ticket, and 409 for an invalid state change.

The normal status path is `Open → InProgress → Resolved → Closed`. A coordinator may reopen `Resolved → InProgress`. Starting work requires an assigned technician. Assigned technicians may start and resolve work; coordinators can manage transitions and close resolved tickets.

Resolution SLA is calendar time from the ticket's original creation: Low 48h, Medium 24h, High 8h, Critical 2h. Changing priority recalculates the due time from original creation; reopening keeps that due time. An unresolved ticket is at risk when 25% or less of its current priority's window remains. It is overdue after the deadline. All timestamps are stored as UTC and formatted for display by the client.

Ticket, comment and history rows are saved in one EF `SaveChangesAsync` operation per mutation. The seed has six synthetic users and 150 tickets with different states, priorities, categories, comments and history. Its clock-relative SLA examples are generated at initialization.
