/**
 * Keystroke sanitizers for fields that mean "words only" or "numbers only" to a user.
 * Applied in onChange so typing, pasting, and autofill are all covered the same way.
 */

/** Letters, spaces, hyphens, and apostrophes — for person names (e.g. "Mary-Anne O'Neil"). */
export function sanitizeLettersOnly(value: string): string {
  return value.replace(/[^A-Za-z\s'-]/g, "");
}

/** Letters, digits, and hyphens — employee numbers are alphanumeric IDs (e.g. "LM-1024"), not pure numbers. */
export function sanitizeEmployeeNumber(value: string): string {
  return value.replace(/[^A-Za-z0-9-]/g, "");
}

/** Digits plus common phone formatting characters (spaces, +, -, parentheses). */
export function sanitizePhoneNumber(value: string): string {
  return value.replace(/[^0-9+()\s-]/g, "");
}
