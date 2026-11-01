#!/bin/sh
# Merges custom_files into game/csgo on server startup.
# Controlled by the CUSTOM_FILES_ENABLED egg variable (1 = on, 0 = off).
# Every run is also appended to custom_files.log (last 1000 lines are kept).

cd "$(dirname "$0")" || exit 0

LOG=custom_files.log

log() {
  echo "[custom_files] $1"
  echo "$(date '+%Y-%m-%d %H:%M:%S') $1" >> "$LOG"
}

# Keep the log file small
if [ -f "$LOG" ]; then
  tail -n 1000 "$LOG" > "$LOG.tmp" && mv "$LOG.tmp" "$LOG"
fi

echo "" >> "$LOG"
log "---- startup ----"

if [ "$CUSTOM_FILES_ENABLED" != "1" ]; then
  log "disabled"
  exit 0
fi

CFG=addons/counterstrikesharp/configs
mkdir -p "custom_files/$CFG"

# Pull back config files generated on the server that are not in custom_files yet (never overwrites).
# Skips CSS example/readme files and runtime data (databases, logs) so they are never frozen.
if [ -d "game/csgo/$CFG" ]; then
  (cd "game/csgo/$CFG" && find . -type f \
    ! -name '*.example.json' ! -name 'README*' \
    ! -name '*.sqlite*' ! -name '*.db' ! -name '*.db-*' ! -name '*.log' \
    ! -path '*/logs/*' ! -path '*/data/*') | while IFS= read -r f; do
    f=${f#./}
    dst="custom_files/$CFG/$f"
    if [ ! -e "$dst" ]; then
      mkdir -p "$(dirname "$dst")"
      if cp "game/csgo/$CFG/$f" "$dst"; then
        log "pulled back: configs/$f"
      else
        log "pull back FAILED: configs/$f"
      fi
    fi
  done
fi

# Merge custom_files over game/csgo
if cp -RT custom_files/ game/csgo/; then
  log "copied"
else
  log "copy FAILED"
fi
