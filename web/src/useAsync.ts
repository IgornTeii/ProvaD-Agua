import { useCallback, useEffect, useState } from "react";

interface AsyncState<T> {
  data: T | null;
  error: Error | null;
  loading: boolean;
}

export function useAsync<T>(load: () => Promise<T>, deps: unknown[], refreshMs?: number) {
  const [state, setState] = useState<AsyncState<T>>({ data: null, error: null, loading: true });
  const run = useCallback(load, deps);

  useEffect(() => {
    let active = true;

    const execute = () =>
      run()
        .then((data) => active && setState({ data, error: null, loading: false }))
        .catch((error: Error) => active && setState((s) => ({ ...s, error, loading: false })));

    setState((s) => ({ ...s, loading: true }));
    execute();

    const timer = refreshMs ? setInterval(execute, refreshMs) : undefined;
    return () => {
      active = false;
      if (timer) clearInterval(timer);
    };
  }, [run, refreshMs]);

  return state;
}
