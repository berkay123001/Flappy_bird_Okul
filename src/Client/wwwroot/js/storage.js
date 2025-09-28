const STORAGE_UNAVAILABLE_ERROR = "StorageUnavailableError";

function getStorage() {
  if (typeof window === "undefined" || typeof window.localStorage === "undefined") {
    throw new Error(STORAGE_UNAVAILABLE_ERROR);
  }

  return window.localStorage;
}

function withStorage(operation) {
  try {
    const storage = getStorage();
    return operation(storage);
  } catch (error) {
    throw new Error(STORAGE_UNAVAILABLE_ERROR, { cause: error });
  }
}

export function getNumber(key, defaultValue) {
  if (typeof key !== "string" || key.length === 0) {
    throw new Error("InvalidStorageKey");
  }

  return withStorage((storage) => {
    const raw = storage.getItem(key);
    if (raw === null) {
      return defaultValue;
    }

    const parsed = Number.parseInt(raw, 10);
    if (!Number.isFinite(parsed) || Number.isNaN(parsed)) {
      storage.removeItem(key);
      return defaultValue;
    }

    return parsed;
  });
}

export function setNumber(key, value) {
  if (typeof key !== "string" || key.length === 0) {
    throw new Error("InvalidStorageKey");
  }

  const normalized = Number.isFinite(value) ? Math.trunc(value) : 0;

  withStorage((storage) => {
    storage.setItem(key, String(normalized));
  });
}

export function remove(key) {
  if (typeof key !== "string" || key.length === 0) {
    throw new Error("InvalidStorageKey");
  }

  withStorage((storage) => {
    storage.removeItem(key);
  });
}
