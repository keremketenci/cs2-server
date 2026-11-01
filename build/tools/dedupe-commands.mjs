// Applies the user's "keep only one place" choices to the plugin configs inside custom_files.
// Usage: node dedupe-commands.mjs <custom_files/addons/counterstrikesharp>
import fs from 'node:fs';
import path from 'node:path';

const root = process.argv[2];
const readJson = (p) => JSON.parse(fs.readFileSync(p, 'utf8').replace(/^\s*\/\/.*$/gm, ''));
const header = (p) => fs.readFileSync(p, 'utf8').match(/^(\s*\/\/.*\n)*/)[0];
const writeJson = (p, obj, keepHeader = false) =>
  fs.writeFileSync(p, (keepHeader ? header(p) : '') + JSON.stringify(obj, null, 2) + '\n');

// GameModeManager: Utility Practice uses the new mg_utility group (official maps + utility workshop maps)
const gmmPath = path.join(root, 'configs/plugins/GameModeManager/GameModeManager.json');
const gmm = readJson(gmmPath);
gmm.GameModes.List.find((m) => m.Name === 'Utility Practice').MapGroups = ['mg_utility'];
writeJson(gmmPath, gmm);

// SimpleAdmin: Change Map menu keeps only the official maps, workshop maps now live in GameModeManager groups
const saPath = path.join(root, 'configs/plugins/CS2-SimpleAdmin/CS2-SimpleAdmin.json');
const sa = readJson(saPath);
sa.WorkshopMaps = {};
writeJson(saPath, sa, true);

// CustomCommands: buy anywhere and respawn are handled by !settings (buying, one_round)
const ccPath = path.join(root, 'plugins/CustomCommands/Commands.json');
const removed = ['mp_buy_anywhere_on,ba1', 'mp_buy_anywhere_off,ba0', 'mp_respawn_on_death_on,rd1', 'mp_respawn_on_death_off,rd0'];
const cc = JSON.parse(fs.readFileSync(ccPath, 'utf8')).filter((c) => !removed.includes(c.Command));
const help = cc[0].Message;
const swapLine = help.findIndex((l) => l.includes('!ba1'));
help[swapLine] = '{LIME}!swapteams {GREY}| {LIME}!pm {DEFAULT}/ {LIME}!upm {DEFAULT}pause {GREY}| {LIME}!ch1 {DEFAULT}/ {LIME}!ch0 {DEFAULT}cheats';
const ammoLine = help.findIndex((l) => l.includes('!rd1'));
help[ammoLine] = '{LIME}!ia0 {DEFAULT}/ {LIME}!ia1 {DEFAULT}/ {LIME}!ia2 {DEFAULT}infinite ammo (off / +nades / magazine) {GREY}| {DEFAULT}buy anywhere, respawn: {LIME}!settings';
fs.writeFileSync(ccPath, JSON.stringify(cc, null, 2) + '\n');

console.log('utility groups:', gmm.GameModes.List.find((m) => m.Name === 'Utility Practice').MapGroups);
console.log('SimpleAdmin DefaultMaps:', sa.DefaultMaps.join(', '), '| WorkshopMaps:', Object.keys(sa.WorkshopMaps).length);
console.log('CustomCommands left:', cc.map((c) => c.Command).join('  '));
console.log(help[swapLine] + '\n' + help[ammoLine]);
