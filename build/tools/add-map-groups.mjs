// Adds the user's workshop maps (moved from SimpleAdmin's WorkshopMaps) to GameModeManager map groups.
// Usage: node add-map-groups.mjs <path to gamemodes_server.txt>
import fs from 'node:fs';

const file = process.argv[2];
let text = fs.readFileSync(file, 'utf8');

const mapLine = (map) => `\t\t\t\t"${map}"\t\t""\n`;

function addToGroup(group, maps) {
  const re = new RegExp(`(\\t\\t"${group}"\\r?\\n\\t\\t\\{[\\s\\S]*?"maps"\\r?\\n\\t\\t\\t\\{\\r?\\n)`);
  if (!re.test(text)) throw new Error(`group not found: ${group}`);
  const missing = maps.filter((m) => !text.includes(`"${m}"`));
  text = text.replace(re, `$1${missing.map(mapLine).join('')}`);
}

addToGroup('mg_awp', ['workshop/3367343580/awp_lego_2', 'workshop/3206461682/awp_lego_2_no_deagle']);
addToGroup('mg_aim', ['workshop/3199551320/aim_redline', 'workshop/3070244462/aim_botz']);

if (!text.includes('"mg_utility"')) {
  const utilityMaps = [
    'de_ancient', 'de_anubis', 'de_inferno', 'de_mirage', 'de_nuke', 'de_dust2', 'de_overpass',
    'workshop/3340925621/nuke_utility', 'workshop/3332771164/inferno_utility',
    'workshop/3323457022/vertigo_utility', 'workshop/3351615311/ancient_utility',
    'workshop/3360435602/dust2_utility', 'workshop/3312981414/mirage_utility',
    'workshop/3368313759/anubis_utility', 'workshop/3566472167/overpass_utility',
    'workshop/3569811497/train_utility',
  ];
  const block = '\t\t"mg_utility"\n\t\t{\n\t\t\t"name"\t\t\t\t\t"mg_utility"\n\t\t\t"displayname"\t\t\t"Utility"\n'
    + '\t\t\t"maps"\n\t\t\t{\n' + utilityMaps.map(mapLine).join('') + '\t\t\t}\n\t\t}\n';
  const anchor = /(\t"mapgroups"\r?\n\t\{\r?\n)/;
  if (!anchor.test(text)) throw new Error('"mapgroups" section not found');
  text = text.replace(anchor, `$1${block}`);
}

fs.writeFileSync(file, text);
console.log('done');
