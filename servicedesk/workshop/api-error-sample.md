# Próbka błędu do zadania 2B

Scenariusz: zalogowany zgłaszający otwiera w przeglądarce adres szczegółów zgłoszenia należącego do innego zgłaszającego. UI wyświetla komunikat o braku dostępu. Żądanie `GET /api/tickets/{id}` zwraca 404 z komunikatem `Ticket not found`.

Próbka opisuje obserwację do prześledzenia w kodzie. Nie zakładaj, że sam komunikat UI wskazuje przyczynę lub że 404 oznacza brak rekordu w bazie.
