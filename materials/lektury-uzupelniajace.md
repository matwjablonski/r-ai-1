# Lektury uzupełniające do warsztatu AI-Powered Development

Materiały pogłębiają tematy ćwiczone w ServiceDesk: poznawanie projektu, pracę z agentem, dobór kontekstu, integracje i ocenę zmian. Większość tekstów jest po angielsku. Wybierz sekcję związaną z tematem, który chcesz przećwiczyć; nie trzeba czytać wszystkiego po kolei.

## Poznanie projektu i instrukcje dla agenta

- [Best practices for Claude Code — Anthropic](https://code.claude.com/docs/en/best-practices) — jak podawać agentowi kontekst, dzielić pracę i sprawdzać wynik.
- [How Claude remembers your project — Anthropic](https://code.claude.com/docs/en/memory) — zakres i utrzymanie instrukcji projektu w `CLAUDE.md`.
- [Markdown. Skromny język, który napisał Internet — Mateusz Jabłoński](https://mateuszjablonski.com/blog/markdown-skromny-jezyk-ktory-napisal-internet) — polski tekst o formacie używanym w dokumentacji i instrukcjach dla agentów.

## Praca z agentem i zmiana kodu

- [Building effective agents — Anthropic](https://www.anthropic.com/research/building-effective-agents) — podstawowe wzorce przepływów pracy i agentów.
- [Humans and Agents in Software Engineering Loops — Martin Fowler](https://martinfowler.com/articles/exploring-gen-ai/humans-and-agents.html) — rola człowieka w podejmowaniu decyzji i ocenie pracy agenta.
- [First run the tests — Simon Willison](https://simonwillison.net/guides/agentic-engineering-patterns/first-run-the-tests/) — dlaczego warto poznać stan testów przed zmianą.
- [Characterization Testing — Michael Feathers](https://michaelfeathers.silvrback.com/characterization-testing) — jak utrwalić faktyczne zachowanie istniejącego systemu przed refaktoryzacją.
- [Working Effectively With Characterization Tests — Alberto Savoia](https://www.artima.com/weblogs/viewpost.jsp?thread=198296) — praktyczny przykład testów charakterystyki w Javie.

## Kontekst, pamięć i wyszukiwanie

- [Effective context engineering for AI agents — Anthropic](https://www.anthropic.com/research/effective-context-engineering-for-ai-agents) — selekcja informacji, odczyt na żądanie i kompakcja kontekstu.
- [Context Engineering for Coding Agents — Martin Fowler](https://martinfowler.com/articles/exploring-gen-ai/context-engineering-coding-agents.html) — wpływ instrukcji, narzędzi i historii rozmowy na kontekst agenta.
- [Architecting efficient context-aware multi-agent framework for production — Google Developers](https://developers.googleblog.com/en/architecting-efficient-context-aware-multi-agent-framework-for-production/) — bardziej zaawansowany materiał o selekcji kontekstu i kosztach pracy wielu agentów.
- [The Representation Problem: Why RAG vs Agentic Search Is the Wrong Debate — Yaohua Chen](https://dev.to/chen115y/the-representation-problem-why-rag-vs-agentic-search-is-the-wrong-debate-4j8f) — dlaczego sposób reprezentacji wiedzy powinien wpływać na wybór metody wyszukiwania. To argument autora, nie uniwersalny benchmark.
- [Is Grep All You Need? — Sen i in.](https://arxiv.org/abs/2605.15184) — badanie wyszukiwania w historii rozmów; nie jest porównaniem metod szukania w repozytoriach kodu.
- [Benchmarking AI Agent Memory: Is a Filesystem All You Need? — Signals](https://signals.aktagon.com/articles/2026/02/benchmarking-ai-agent-memory-is-a-filesystem-all-you-need/) — omówienie testu pamięci agenta opartej na plikach i prostym wyszukiwaniu.
- [Claude Code: How Anthropic's Engineers Build With Claude — Latent Space](https://www.latent.space/p/claude-code) — wywiad z Borisem Cherny'm; zawiera wyjaśnienie decyzji o odejściu od prototypu wektorowego indeksu kodu w Claude Code.

## Skills, hooks i subagenci

- [Extend Claude with skills — Anthropic](https://code.claude.com/docs/en/skills) — tworzenie i zakres umiejętności agenta.
- [Automate actions with hooks — Anthropic](https://code.claude.com/docs/en/hooks-guide) — zdarzenia i automatyzacja działań w Claude Code.
- [Create custom subagents — Anthropic](https://code.claude.com/docs/en/sub-agents) — definiowanie subagentów i przydzielanie im narzędzi.

## MCP i testowanie w przeglądarce

- [Connect Claude Code to tools via MCP — Anthropic](https://code.claude.com/docs/en/mcp) — podłączanie serwerów MCP i zarządzanie narzędziami.
- [Playwright MCP — Microsoft](https://playwright.dev/docs/getting-started-mcp) — automatyzacja przeglądarki z użyciem MCP.
- [Playwright best practices — Microsoft](https://playwright.dev/docs/best-practices) — testowanie zachowań widocznych dla użytkownika i izolacja testów.
- [MCP Security — OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/cheatsheets/MCP_Security_Cheat_Sheet.html) — ryzyka związane z zewnętrznymi narzędziami i ograniczaniem ich uprawnień.

## Kontrola jakości zmian

- [The role of developer skills in agentic coding — Martin Fowler / Thoughtworks](https://martinfowler.com/articles/exploring-gen-ai/13-role-of-developer-skills.html) — przykłady błędnych diagnoz i zbędnych zmian proponowanych przez agenta.
- [Assessing internal quality while coding with an agent — Martin Fowler / Thoughtworks](https://martinfowler.com/articles/exploring-gen-ai/ccmenu-quality.html) — studium przypadku oceny jakości kodu po pracy z agentem.
- [ReviewBench: An open benchmark for AI code review — GitHub](https://github.blog/ai-and-ml/reviewbench-an-open-benchmark-for-ai-code-review/) — jak odróżniać trafne uwagi reviewera od szumu.
