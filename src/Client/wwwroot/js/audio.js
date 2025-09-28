const AUDIO_AUTOPLAY_ERROR = "AudioAutoplayBlocked";
const AUDIO_MISSING_ERROR = "AudioMissingError";

const audioCache = new Map();
const activePlayers = new Set();

function toAbsolutePath(root, fileName) {
  if (typeof root !== "string" || root.length === 0) {
    return fileName;
  }

  const trimmedRoot = root.endsWith("/") ? root.slice(0, -1) : root;
  return `${trimmedRoot}/${fileName}`;
}

function loadAudio(effectName, sourcePath) {
  return new Promise((resolve, reject) => {
    const audio = new Audio();
    audio.preload = "auto";
    audio.src = sourcePath;

    const cleanup = () => {
      audio.removeEventListener("canplaythrough", handleCanPlay);
      audio.removeEventListener("error", handleError);
    };

    const handleCanPlay = () => {
      cleanup();
      audioCache.set(effectName, audio);
      resolve();
    };

    const handleError = () => {
      cleanup();
      reject(new Error(AUDIO_MISSING_ERROR));
    };

    audio.addEventListener("canplaythrough", handleCanPlay, { once: true });
    audio.addEventListener("error", handleError, { once: true });

    // Kick off loading
    audio.load();
  });
}

function clonePlayer(effectName) {
  const baseAudio = audioCache.get(effectName);
  if (!baseAudio) {
    throw new Error(AUDIO_MISSING_ERROR);
  }

  const clone = baseAudio.cloneNode(true);
  clone.preload = "auto";
  clone.currentTime = 0;

  clone.addEventListener(
    "ended",
    () => {
      activePlayers.delete(clone);
    },
    { once: true }
  );

  clone.addEventListener(
    "error",
    () => {
      activePlayers.delete(clone);
    },
    { once: true }
  );

  activePlayers.add(clone);
  return clone;
}

export async function preloadAssets(manifest) {
  if (!manifest || typeof manifest !== "object") {
    throw new Error(AUDIO_MISSING_ERROR);
  }

  const { root, files } = manifest;
  const entries = Object.entries(files ?? {});

  const loadTasks = entries.map(([effectName, fileName]) =>
    loadAudio(effectName, toAbsolutePath(root ?? "", fileName))
  );

  if (loadTasks.length === 0) {
    return;
  }

  await Promise.all(loadTasks);
}

export async function playEffect(effectName) {
  if (typeof effectName !== "string" || effectName.length === 0) {
    throw new Error(AUDIO_MISSING_ERROR);
  }

  const player = clonePlayer(effectName);

  try {
    await player.play();
  } catch (error) {
    activePlayers.delete(player);

    if (error && typeof error === "object") {
      const name = error.name ?? "";
      const code = error.code ?? "";
      if (name === "NotAllowedError" || code === 0) {
        throw new Error(AUDIO_AUTOPLAY_ERROR);
      }
    }

    throw error;
  }
}

export function stopAll() {
  for (const player of activePlayers) {
    try {
      player.pause();
      player.currentTime = 0;
    } catch (error) {
      console.debug("Failed to stop audio player", error);
    }
  }

  activePlayers.clear();
}
