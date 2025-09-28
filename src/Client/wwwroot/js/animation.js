const loops = new Map();
let nextLoopId = 1;
let activeLoopId = null;

function ensureLoopArguments(dotNetRef, callbackName) {
  if (!dotNetRef || typeof dotNetRef.invokeMethodAsync !== "function") {
    throw new Error("InvalidDotNetReference");
  }

  if (typeof callbackName !== "string" || callbackName.length === 0) {
    throw new Error("InvalidCallbackName");
  }
}

function cancelLoop(loopId) {
  const entry = loops.get(loopId);
  if (!entry) {
    return;
  }

  cancelAnimationFrame(entry.rafId);
  loops.delete(loopId);
  if (activeLoopId === loopId) {
    activeLoopId = null;
  }
}

function createLoop(dotNetRef, callbackName) {
  const loopId = nextLoopId++;

  const frameCallback = async (timestamp) => {
    const current = loops.get(loopId);
    if (!current) {
      return;
    }

    current.rafId = requestAnimationFrame(frameCallback);

    try {
      await dotNetRef.invokeMethodAsync(callbackName, timestamp);
    } catch (error) {
      cancelLoop(loopId);
      console.error("Animation loop callback failed", error);
    }
  };

  const rafId = requestAnimationFrame(frameCallback);
  loops.set(loopId, { dotNetRef, callbackName, rafId });
  activeLoopId = loopId;
  return loopId;
}

export function startLoop(dotNetRef, callbackName) {
  ensureLoopArguments(dotNetRef, callbackName);

  if (activeLoopId !== null) {
    cancelLoop(activeLoopId);
  }

  return createLoop(dotNetRef, callbackName);
}

export function stopLoop(loopId) {
  if (typeof loopId !== "number") {
    return;
  }

  cancelLoop(loopId);
}

export function stopAllLoops() {
  for (const loopId of [...loops.keys()]) {
    cancelLoop(loopId);
  }
}
