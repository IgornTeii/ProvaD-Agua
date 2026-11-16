import { useEffect, useState } from "react";

const SCALES = [1, 1.125, 1.25, 1.4];
const STORAGE_KEY = "prova-dagua-preferencias";

interface Preferences {
  scaleIndex: number;
  highContrast: boolean;
}

function load(): Preferences {
  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? "null");
    if (saved && typeof saved.scaleIndex === "number") return saved;
  } catch {
    return { scaleIndex: 0, highContrast: false };
  }
  return { scaleIndex: 0, highContrast: false };
}

export function usePreferences() {
  const [preferences, setPreferences] = useState<Preferences>(load);

  useEffect(() => {
    const root = document.documentElement;
    root.style.fontSize = `${SCALES[preferences.scaleIndex] * 112.5}%`;
    root.dataset.contrast = preferences.highContrast ? "high" : "normal";
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(preferences));
    } catch {
      return;
    }
  }, [preferences]);

  return {
    canGrow: preferences.scaleIndex < SCALES.length - 1,
    canShrink: preferences.scaleIndex > 0,
    highContrast: preferences.highContrast,
    grow: () => setPreferences((p) => ({ ...p, scaleIndex: Math.min(p.scaleIndex + 1, SCALES.length - 1) })),
    shrink: () => setPreferences((p) => ({ ...p, scaleIndex: Math.max(p.scaleIndex - 1, 0) })),
    toggleContrast: () => setPreferences((p) => ({ ...p, highContrast: !p.highContrast })),
  };
}
