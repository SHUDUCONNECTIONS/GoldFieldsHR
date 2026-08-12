import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Search } from "lucide-react";
import { NAV_ITEMS, type NavItem } from "../config/nav";

export function HeaderSearch() {
  const navigate = useNavigate();
  const [query, setQuery] = useState("");
  const [isOpen, setIsOpen] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(0);
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const trimmed = query.trim().toLowerCase();
  const results: NavItem[] = trimmed
    ? NAV_ITEMS.filter((item) => item.label.toLowerCase().includes(trimmed))
    : NAV_ITEMS.filter((item) => item.path !== "/");

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  useEffect(() => {
    function handleShortcut(event: KeyboardEvent) {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        inputRef.current?.focus();
        setIsOpen(true);
      }
    }
    window.addEventListener("keydown", handleShortcut);
    return () => window.removeEventListener("keydown", handleShortcut);
  }, []);

  useEffect(() => {
    setHighlightedIndex(0);
  }, [query]);

  function goTo(item: NavItem) {
    navigate(item.path);
    setQuery("");
    setIsOpen(false);
    inputRef.current?.blur();
  }

  return (
    <div ref={containerRef} className="relative hidden sm:block">
      <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
      <input
        ref={inputRef}
        type="search"
        value={query}
        onFocus={() => setIsOpen(true)}
        onChange={(e) => setQuery(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === "ArrowDown") {
            e.preventDefault();
            setHighlightedIndex((i) => Math.min(i + 1, results.length - 1));
          } else if (e.key === "ArrowUp") {
            e.preventDefault();
            setHighlightedIndex((i) => Math.max(i - 1, 0));
          } else if (e.key === "Enter") {
            e.preventDefault();
            if (results[highlightedIndex]) goTo(results[highlightedIndex]);
          } else if (e.key === "Escape") {
            setIsOpen(false);
            inputRef.current?.blur();
          }
        }}
        placeholder="Jump to a module..."
        className="w-72 rounded-md border border-slate-300 bg-slate-50 py-1.5 pl-9 pr-14 text-sm text-slate-700 focus:border-red-500 focus:outline-none"
      />
      {!isOpen && !query && (
        <kbd className="pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2 rounded border border-slate-300 bg-white px-1.5 py-0.5 text-[10px] font-medium text-slate-400">
          Ctrl K
        </kbd>
      )}

      {isOpen && (
        <div className="absolute left-0 z-50 mt-2 w-72 rounded-lg border border-slate-200 bg-white shadow-lg">
          {results.length === 0 ? (
            <p className="px-4 py-6 text-center text-sm text-slate-500">No modules match &ldquo;{query}&rdquo;.</p>
          ) : (
            <ul className="max-h-80 overflow-y-auto py-1">
              {results.map((item, index) => {
                const Icon = item.icon;
                return (
                  <li key={item.path}>
                    <button
                      type="button"
                      onMouseEnter={() => setHighlightedIndex(index)}
                      onClick={() => goTo(item)}
                      className={`flex w-full items-center gap-2.5 px-4 py-2 text-left text-sm ${
                        index === highlightedIndex ? "bg-red-50 text-red-700" : "text-slate-700"
                      }`}
                    >
                      <Icon className="h-4 w-4 shrink-0" />
                      {item.label}
                    </button>
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}
