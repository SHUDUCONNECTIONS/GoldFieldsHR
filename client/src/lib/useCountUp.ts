import { useEffect, useRef, useState } from "react";

const DURATION_MS = 800;

function easeOutCubic(t: number): number {
  return 1 - Math.pow(1 - t, 3);
}

/** Animates from the previous value to `target` whenever `target` changes. */
export function useCountUp(target: number | null, decimals = 0): number | null {
  const [value, setValue] = useState(target);
  const fromRef = useRef(0);
  const frameRef = useRef<number | undefined>(undefined);

  useEffect(() => {
    if (target === null) {
      setValue(null);
      return;
    }

    const from = fromRef.current;
    const startTime = performance.now();

    function tick(now: number) {
      const elapsed = now - startTime;
      const progress = Math.min(1, elapsed / DURATION_MS);
      const eased = easeOutCubic(progress);
      const next = from + (target! - from) * eased;
      const factor = 10 ** decimals;
      setValue(Math.round(next * factor) / factor);

      if (progress < 1) {
        frameRef.current = requestAnimationFrame(tick);
      } else {
        fromRef.current = target!;
      }
    }

    frameRef.current = requestAnimationFrame(tick);
    return () => {
      if (frameRef.current) cancelAnimationFrame(frameRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [target, decimals]);

  return value;
}
