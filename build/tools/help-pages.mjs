// Rebuilds the CustomCommands help: !help shows the categories, each category has its own page.
// Usage: node help-pages.mjs <custom_files/addons/counterstrikesharp/plugins/CustomCommands/Commands.json>
import fs from 'node:fs';

const file = process.argv[2];
const commands = JSON.parse(fs.readFileSync(file, 'utf8'));

const header = (title) => `{GOLD}========== {BLUE}${title} {GOLD}==========`;
const row = (cmd, desc) => ` {LIME}${cmd} {GREY}- {DEFAULT}${desc}`;
const adminRow = (cmd, desc) => ` {RED}* {LIME}${cmd} {GREY}- {DEFAULT}${desc}`;
const note = (text) => ` {GREY}${text}`;

const pages = [
  {
    command: 'help,yardim,komutlar,commands',
    title: 'Command list',
    lines: [
      header('SERVER HELP'),
      note('Type a category command to see its list:'),
      row('!hmaps', 'modes, maps and voting'),
      row('!hgame', 'game toggles: bunny, bots, parachute ...'),
      row('!hmatch', 'FACEIT match: ready, pause, knife'),
      row('!hprac', 'utility practice: rethrow, bots, lineups'),
      row('!hdm', 'deathmatch weapons'),
      row('!hretake', 'retake: guns menu, votes'),
      row('!sahelp', 'SimpleAdmin: players, punish, fun (2 pages)'),
      ' {RED}*{GREY} = admin only   |   {LIME}.{GREY} commands work in FACEIT and Utility modes',
    ],
  },
  {
    command: 'hmaps',
    title: 'Help: modes and maps',
    lines: [
      header('MODES & MAPS'),
      row('!rtv', 'vote for the next map or mode'),
      row('!nominate', 'add a map or mode to the next vote'),
      row('!nextmap {GREY}/ {LIME}!nextmode', 'what comes next'),
      row('!currentmap {GREY}/ {LIME}!currentmode', 'what is playing now'),
      row('!timeleft', 'time left on this map'),
      row('!game', 'player menu'),
      row('!changemenu', 'change how menus look'),
      adminRow('!modes', 'switch the game mode (loads its default map)'),
      adminRow('!modes2 {GREY}/ {LIME}!allmodes', 'switch the game mode, stay on this map'),
      adminRow('!hmodes', 'one command per mode: !awponly, !retake, !wingman ...'),
      adminRow('!maps', 'change the map (maps of the current mode)'),
      adminRow('!maps2 {GREY}/ {LIME}!allmaps', 'any server map by category, keeps the current mode'),
      adminRow('!map <name>', 'change to a map by name'),
      adminRow('!settings', 'game settings: buy anywhere, respawn, bots ...'),
    ],
  },
  {
    command: 'hmodes',
    title: 'Help: mode shortcuts',
    lines: [
      header('MODE SHORTCUTS {RED}(admin)'),
      row('!freeplay {GREY}/ {LIME}!fpr', 'Free Play / Free Play Rounds'),
      row('!casual {GREY}/ {LIME}!comp {GREY}/ {LIME}!wingman', 'Casual / Competitive / Wingman'),
      row('!retake', 'Retake'),
      row('!faceit {GREY}/ {LIME}!faceitffoff', 'FACEIT FF On / FACEIT FF Off'),
      row('!utility', 'Utility Practice'),
      row('!deathmatch {GREY}/ {LIME}!gungame {GREY}/ {LIME}!armsrace', 'Deathmatch / Gun Game / Arms Race'),
      row('!rush {GREY}/ {LIME}!awponly {GREY}/ {LIME}!aim', 'Rush / AWP / Aim'),
      note('Loads the default map of the mode. Same map: {LIME}!modes2'),
    ],
  },
  {
    command: 'hgame',
    title: 'Help: game toggles',
    lines: [
      header('GAME TOGGLES {RED}(admin)'),
      row('!bon {GREY}/ {LIME}!boff', 'bunny hop on / off'),
      row('!abon {GREY}/ {LIME}!aboff', 'team auto balance on / off'),
      row('!pon {GREY}/ {LIME}!poff', 'parachute on / off'),
      row('!bk', 'kick all bots'),
      row('!bat {GREY}/ {LIME}!bac', 'add a T / CT bot'),
      row('!rg', 'restart the game'),
      row('!we', 'end the warmup'),
      row('!swapteams', 'swap the teams'),
      row('!pm {GREY}/ {LIME}!upm', 'pause at next round start / unpause'),
      row('!ch1 {GREY}/ {LIME}!ch0', 'cheats on / off'),
      row('!ia0 {GREY}/ {LIME}!ia1 {GREY}/ {LIME}!ia2', 'infinite ammo: off / with grenades / magazine only'),
      row('!rec {GREY}/ {LIME}!norec', 'record the next match / stop recording'),
      note('More toggles (buy anywhere, respawn, bots, scout only ...): {LIME}!settings'),
    ],
  },
  {
    command: 'hmatch',
    title: 'Help: FACEIT match',
    lines: [
      header('FACEIT MATCH'),
      row('.ready {GREY}/ {LIME}.r', 'ready up'),
      row('.unready', 'not ready anymore'),
      row('.stay {GREY}/ {LIME}.switch', 'knife winners pick their side'),
      row('.pause {GREY}/ {LIME}.unpause', 'pause (both teams must unpause)'),
      row('.tech', 'technical pause'),
      row('.tac', 'tactical timeout'),
      adminRow('.start', 'force start the match'),
      adminRow('.rr', 'restart the match'),
      adminRow('.endmatch', 'end the match'),
      adminRow('.rk', 'knife round on / off'),
      adminRow('.fp {GREY}/ {LIME}.fup', 'force pause / unpause'),
    ],
  },
  {
    command: 'hprac',
    title: 'Help: utility practice',
    lines: [
      header('UTILITY PRACTICE'),
      row('.rethrow', 'throw your last grenade again'),
      row('.last', 'go back to where you threw it'),
      row('.savenade <name> {GREY}/ {LIME}.loadnade', 'save / load a lineup'),
      row('.listnades', 'your saved lineups'),
      row('.bot {GREY}/ {LIME}.crouchbot', 'place a bot where you look'),
      row('.nobots', 'remove the bots'),
      row('.spawn {GREY}/ {LIME}.ctspawn {GREY}/ {LIME}.tspawn', 'teleport to a spawn'),
      row('.clear', 'clear smokes and fires'),
      row('.ff', 'fast forward'),
      row('.noflash', 'flashbangs do not blind you'),
      row('.god', 'god mode'),
      row('.traj {GREY}/ {LIME}.impacts', 'grenade trajectory / bullet impacts'),
    ],
  },
  {
    command: 'hdm',
    title: 'Help: deathmatch',
    lines: [
      header('DEATHMATCH'),
      row('!gun {GREY}/ {LIME}!w {GREY}/ {LIME}!g', 'choose your weapons'),
      row('!dm', 'deathmatch settings'),
      row('!ak !m4 !m4s !awp !scout', 'quick rifle'),
      row('!deagle !usp !glock !mac', 'quick pistol / SMG'),
    ],
  },
  {
    command: 'hretake',
    title: 'Help: retake',
    lines: [
      header('RETAKE'),
      note('Bomb is already planted: Ts hold the site, CTs retake it. First 2 rounds are pistol rounds.'),
      row('!guns {GREY}/ {LIME}!pistols {GREY}/ {LIME}!awp', 'loadout menu (or type {LIME}guns{DEFAULT} in chat)'),
      row('!vp {GREY}/ {LIME}!vph', 'vote pistol rounds / pistols + headshots only'),
      row('!vawp {GREY}/ {LIME}!vrifles {GREY}/ {LIME}!vhs', 'vote AWP / rifles / headshots only rounds'),
      row('!voices', 'bombsite voice announcements on / off'),
      adminRow('!scramble', 'scramble the teams next round'),
      adminRow('!scron {GREY}/ {LIME}!scroff', 'auto scramble after 5 T wins on / off'),
      adminRow('!roton {GREY}/ {LIME}!rotoff', 'round-end rotation + team balance on / off'),
      adminRow('!forcebombsite A|B {GREY}/ {LIME}!forcebombsitestop', 'play one site only {GREY}(owner)'),
      adminRow('!forcevawp', 'force a vote: !force<vote> {GREY}(owner)'),
      adminRow('!skip_pistol', 'skip the pistol rounds {GREY}(owner)'),
      note('The menu needs the HUD addon players download on connect.'),
    ],
  },
  {
    command: 'sahelp,hadmin',
    title: 'Help: SimpleAdmin 1/2',
    lines: [
      header('SIMPLEADMIN 1/2 {RED}(admin)'),
      row('!admin', 'admin menu: players, fun, server'),
      row('!kick {GREY}/ {LIME}!ban {GREY}/ {LIME}!unban <player>', 'kick / ban / unban'),
      row('!mute {GREY}/ {LIME}!gag {GREY}/ {LIME}!silence', 'block voice / chat / both (!un... undoes)'),
      row('!warn {GREY}/ {LIME}!unwarn {GREY}/ {LIME}!warns', 'warnings'),
      row('!slay {GREY}/ {LIME}!slap {GREY}/ {LIME}!respawn', 'kill / slap / respawn'),
      row('!team <player> <t|ct|spec>', 'move a player'),
      row('!rename <player> <name>', 'rename a player'),
      row('!who {GREY}/ {LIME}!players {GREY}/ {LIME}!disconnected', 'player info'),
      row('!history <player>', 'penalty history'),
      note('Target: name, #userid or {LIME}@all @t @ct @alive @dead @bots @human @me'),
      note('Next page: {LIME}!sahelp2'),
    ],
  },
  {
    command: 'sahelp2',
    title: 'Help: SimpleAdmin 2/2',
    lines: [
      header('SIMPLEADMIN 2/2 {RED}(admin)'),
      row('!freeze {GREY}/ {LIME}!unfreeze <target> [sec]', 'e.g. {LIME}!freeze @all 10'),
      row('!noclip {GREY}/ {LIME}!godmode <target>', 'fly / god mode'),
      row('!hp {GREY}/ {LIME}!speed {GREY}/ {LIME}!gravity <target> <value>', 'health / speed / gravity'),
      row('!give <target> <weapon> {GREY}/ {LIME}!strip', 'give / remove weapons'),
      row('!money {GREY}/ {LIME}!resize <target> <value>', 'money / model size'),
      row('!tp {GREY}/ {LIME}!bring <target>', 'go to / bring a player'),
      row('!say {GREY}/ {LIME}!csay {GREY}/ {LIME}!hsay {GREY}/ {LIME}!achat', 'chat / center / hint / admins only'),
      row('!rg', 'restart the game'),
      row('!hide', 'hide yourself from the scoreboard'),
      note('Owner only: {LIME}!sarcon <command>{GREY}, {LIME}!addadmin'),
    ],
  },
];

// Old help pages are dropped by their Description, so renamed or removed pages do not stay behind
const others = commands.filter((c) => c.Description !== 'Help page');
const helpEntries = pages.map((p) => ({
  Title: p.title,
  Description: 'Help page',
  Command: p.command,
  Message: p.lines,
  PrintTo: 0,
}));

// !rr belongs to MatchZy in FACEIT/Utility, restart the game with !rg (SimpleAdmin) instead
const restart = others.find((c) => c.Command.startsWith('mp_restartgame'));
if (restart) restart.Command = 'mp_restartgame';

fs.writeFileSync(file, JSON.stringify([...helpEntries, ...others], null, 2) + '\n');

const allowed = new Set('BLUEGREY BLUE DARKBLUE DARKRED DEFAULT GOLD GREEN GREY LIGHTPURPLE LIGHTRED LIME OLIVE ORANGE PURPLE RED SILVER WHITE YELLOW'.split(' '));
const bad = [...JSON.stringify(helpEntries).matchAll(/\{([A-Z]+)\}/g)].map((m) => m[1]).filter((t) => !allowed.has(t));
console.log('help pages:', helpEntries.map((e) => e.Command).join('  '));
console.log('other commands:', others.map((c) => c.Command).join('  '));
console.log('unknown color tags:', bad.length ? [...new Set(bad)] : 'none');
console.log('longest line (chars):', Math.max(...helpEntries.flatMap((e) => e.Message.map((l) => l.replace(/\{[A-Z]+\}/g, '').length))));
