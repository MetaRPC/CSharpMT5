# ===== shortcasts.ps1 =====
# Short labels on top "dotnet run -- ..."

# --- defaults for the current session ---
$script:PF  = 'demo'       # default profile
$script:SYM = 'GBPUSD'     # the default character
$script:TO = 90000         # timeout-ms by default
$ci = [System.Globalization.CultureInfo]::InvariantCulture

function use-pf  { param([string]$p) $script:PF=$p;  Write-Host "Default profile: $p" }
function use-sym { param([string]$s) $script:SYM=$s; Write-Host "Default symbol:  $s" }
function use-to  { param([int]$t)    $script:TO=$t;  Write-Host "Default timeout: $t ms" }

# Basic Runner ("dotnet run -- ...")
function mt5 {
  param([Parameter(ValueFromRemainingArguments=$true)][string[]]$Args)
  dotnet run -- @Args
}

# ========= BASIC =========
function info { param([string]$p=$PF,[int]$t=$TO) mt5 info -p $p --timeout-ms $t }
function positions { param([string]$p=$PF,[int]$t=$TO) mt5 positions -p $p --timeout-ms $t }
Set-Alias pos positions
function ord  { param([string]$p=$PF,[int]$t=$TO) mt5 orders -p $p --timeout-ms $t }
function q    { param([string]$s=$SYM,[string]$p=$PF,[int]$t=$TO) mt5 quote -p $p -s $s --timeout-ms $t }

# ========= SYMBOL =========
function en  { param([string]$s=$SYM,[string]$p=$PF,[int]$t=$TO) mt5 symbol ensure -p $p -s $s --timeout-ms $t }
function lim { param([string]$s=$SYM,[string]$p=$PF,[int]$t=$TO) mt5 symbol limits -p $p -s $s --timeout-ms $t }
function sh  { param([string]$s=$SYM,[string]$p=$PF,[int]$t=$TO) mt5 symbol show   -p $p -s $s --timeout-ms $t }

# ========= MARKET ORDERS =========
function b {
  param(
    [string]$s=$SYM, [double]$v=0.10,
    [Nullable[Double]]$sl, [Nullable[Double]]$tp,
    [int]$dev=10, [string]$p=$PF, [int]$t=$TO
  )
  $args = @('buy','-p',$p,'-s',$s,'-v',$v.ToString('G',$ci),'--timeout-ms',$t,'--deviation',$dev)
  if ($PSBoundParameters.ContainsKey('sl')) { $args += @('--sl', $sl.Value.ToString('G',$ci)) }
  if ($PSBoundParameters.ContainsKey('tp')) { $args += @('--tp', $tp.Value.ToString('G',$ci)) }
  mt5 @args
}

function s {
  param(
    [string]$s=$SYM, [double]$v=0.10,
    [Nullable[Double]]$sl, [Nullable[Double]]$tp,
    [int]$dev=10, [string]$p=$PF, [int]$t=$TO
  )
  $args = @('sell','-p',$p,'-s',$s,'-v',$v.ToString('G',$ci),'--timeout-ms',$t,'--deviation',$dev)
  if ($PSBoundParameters.ContainsKey('sl')) { $args += @('--sl', $sl.Value.ToString('G',$ci)) }
  if ($PSBoundParameters.ContainsKey('tp')) { $args += @('--tp', $tp.Value.ToString('G',$ci)) }
  mt5 @args
}

# ========= CLOSE / CANCEL =========
function c {  # close by ticket (volume)
  param([ulong]$t,[Nullable[Double]]$v,[string]$s,$p=$PF,[int]$to=$TO)
  $args=@('close','-p',$p,'-t',$t,'--timeout-ms',$to)
  if ($s) { $args += @('-s',$s) }
  if ($PSBoundParameters.ContainsKey('v')) { $args += @('-v',$v.Value.ToString('G',$ci)) }
  mt5 @args
}

function cs { # close all for symbol (yes)
  param([string]$s=$SYM,[string]$p=$PF,[int]$to=$TO)
  mt5 close-symbol -p $p -s $s --yes --timeout-ms $to
}

function flatten { # close-all (optionally by symbol)
  param([string]$s,[switch]$y=$true,[string]$p=$PF,[int]$to=$TO)
  $args = @('close-all','-p',$p,'--timeout-ms',$to)
  if ($s) { $args += @('--filter-symbol',$s) }
  if ($y) { $args += @('--yes') }
  mt5 @args
}

function x { # cancel pending by ticket
  param([ulong]$t,[string]$s,[string]$p=$PF,[int]$to=$TO)
  $args=@('cancel','-p',$p,'-t',$t,'--timeout-ms',$to)
  if ($s) { $args += @('-s',$s) }
  mt5 @args
}

function ca { # cancel.all (filters optional)
  param([string]$s,[string]$type='any',[string]$p=$PF,[int]$to=$TO)
  $args=@('cancel.all','-p',$p,'--timeout-ms',$to)
  if($s){$args+=@('--symbol',$s)}
  if($type){$args+=@('--type',$type)}
  mt5 @args
}

# ========= PARTIAL CLOSE =========
function cp { # close.partial by exact volume
  param([ulong]$t,[double]$v,[int]$dev=10,[string]$p=$PF,[int]$to=$TO)
  mt5 close.partial -p $p -t $t -v $v.ToString('G',$ci) --deviation $dev --timeout-ms $to
}

function cpp { # close.percent (by percent)
  param([ulong]$t,[double]$pct=50,[int]$dev=10,[string]$p=$PF,[int]$to=$TO)
  mt5 close.percent -p $p -t $t --pct $pct --deviation $dev --timeout-ms $to
}

function ch { # close.half alias
  param([ulong]$t,[int]$dev=10,[string]$p=$PF,[int]$to=$TO)
  mt5 close.half -p $p -t $t --deviation $dev --timeout-ms $to
}

# ========= REVERSE =========
function rv { # reverse by symbol
  param([string]$s=$SYM,[string]$mode='net',[Nullable[Double]]$sl,[Nullable[Double]]$tp,[int]$dev=10,[string]$p=$PF,[int]$to=$TO)
  $args=@('reverse','-p',$p,'-s',$s,'--mode',$mode,'--deviation',$dev,'--timeout-ms',$to)
  if($PSBoundParameters.ContainsKey('sl')){$args+=@('--sl',$sl.Value.ToString('G',$ci))}
  if($PSBoundParameters.ContainsKey('tp')){$args+=@('--tp',$tp.Value.ToString('G',$ci))}
  mt5 @args
}

function rvt { # reverse by ticket
  param([ulong]$t,[Nullable[Double]]$sl,[Nullable[Double]]$tp,[int]$dev=10,[string]$p=$PF,[int]$to=$TO)
  $args=@('reverse.ticket','-p',$p,'-t',$t,'--deviation',$dev,'--timeout-ms',$to)
  if($PSBoundParameters.ContainsKey('sl')){$args+=@('--sl',$sl.Value.ToString('G',$ci))}
  if($PSBoundParameters.ContainsKey('tp')){$args+=@('--tp',$tp.Value.ToString('G',$ci))}
  mt5 @args
}

# ========= SL/TP MODIFY =========
function posmod { # position.modify
  param([ulong]$t,[Nullable[Double]]$sl,[Nullable[Double]]$tp,[string]$p=$PF,[int]$to=$TO)
  $args=@('position.modify','-p',$p,'-t',$t,'--timeout-ms',$to)
  if($PSBoundParameters.ContainsKey('sl')){$args+=@('--sl',$sl.Value.ToString('G',$ci))}
  if($PSBoundParameters.ContainsKey('tp')){$args+=@('--tp',$tp.Value.ToString('G',$ci))}
  mt5 @args
}

function pmp { # position.modify.points
  param([ulong]$t,[Nullable[int]]$slp,[Nullable[int]]$tpp,[ValidateSet('entry','market')][string]$from='entry',[string]$p=$PF,[int]$to=$TO)
  $args=@('position.modify.points','-p',$p,'-t',$t,'--from',$from,'--timeout-ms',$to)
  if($PSBoundParameters.ContainsKey('slp')){$args+=@('--sl-points',$slp.Value)}
  if($PSBoundParameters.ContainsKey('tpp')){$args+=@('--tp-points',$tpp.Value)}
  mt5 @args
}

function be { # breakeven
  param([ulong]$t,[Nullable[Double]]$offset,[Nullable[int]]$offsetPts,[switch]$force,[string]$p=$PF,[int]$to=$TO)
  $args=@('breakeven','-p',$p,'-t',$t,'--timeout-ms',$to)
  if($PSBoundParameters.ContainsKey('offset'))   {$args+=@('--offset',$offset.Value.ToString('G',$ci))}
  if($PSBoundParameters.ContainsKey('offsetPts')){$args+=@('--offset-points',$offsetPts.Value)}
  if($force){$args+=@('--force')}
  mt5 @args
}

# ========= PENDING ORDERS =========
function pdls { param([string]$p=$PF,[int]$to=$TO) mt5 pending list -p $p --timeout-ms $to }

function pm { # pending.modify (universal)
  param(
    [ulong]$t,[string]$type,[Nullable[Double]]$price,[Nullable[Double]]$stop,[Nullable[Double]]$limit,
    [Nullable[Double]]$sl,[Nullable[Double]]$tp,[string]$tif,[Nullable[DateTime]]$expire,
    [string]$p=$PF,[int]$to=$TO
  )
  $args=@('pending.modify','-p',$p,'-t',$t,'--timeout-ms',$to)
  if($type){$args+=@('--type',$type)}
  if($PSBoundParameters.ContainsKey('price')){$args+=@('--price',$price.Value.ToString('G',$ci))}
  if($PSBoundParameters.ContainsKey('stop')) {$args+=@('--stop', $stop.Value.ToString('G',$ci))}
  if($PSBoundParameters.ContainsKey('limit')){$args+=@('--limit',$limit.Value.ToString('G',$ci))}
  if($PSBoundParameters.ContainsKey('sl'))   {$args+=@('--sl',   $sl.Value.ToString('G',$ci))}
  if($PSBoundParameters.ContainsKey('tp'))   {$args+=@('--tp',   $tp.Value.ToString('G',$ci))}
  if($tif){$args+=@('--tif',$tif)}
  if($PSBoundParameters.ContainsKey('expire')){$args+=@('--expire',$expire.Value.ToString('o'))}
  mt5 @args
}

function pmove { # pending.move by points
  param([ulong]$t,[int]$by,[string]$p=$PF,[int]$to=$TO)
  mt5 pending.move -p $p -t $t --by-points $by --timeout-ms $to
}

# ========= TICKET / HISTORY / TOOLS =========
function tsh { param([ulong]$t,[int]$days=30,[string]$p=$PF,[int]$to=$TO) mt5 ticket show -p $p -t $t -d $days --timeout-ms $to }
function hist { param([int]$d=7,[string]$p=$PF,[int]$to=$TO,[ValidateSet('text','json')][string]$o='text') mt5 history -p $p -d $d -o $o --timeout-ms $to }
function hexport {
  param([int]$d=30,[string]$file,[string]$fmt='csv',[string]$s,[string]$p=$PF,[int]$tout=$TO)
  $args=@('history.export','-p',$p,'--days',$d,'--to',$fmt,'--file',$file,'--timeout-ms',$tout)
  if($s){$args+=@('--symbol',$s)}; mt5 @args
}
function lc {
  param([string]$s=$SYM,[double]$riskPct,[int]$slPts,[double]$balance,[double]$minLot=0.01,[double]$lotStep=0.01,[Nullable[Double]]$maxLot,[string]$p=$PF,[int]$to=$TO,[string]$o='text')
  $args=@('lot.calc','--symbol',$s,'--risk-pct',$riskPct,'--sl-points',$slPts,'--balance',$balance,'--min-lot',$minLot,'--lot-step',$lotStep,'-o',$o,'--timeout-ms',$to)
  if($PSBoundParameters.ContainsKey('maxLot')){$args+=@('--max-lot',$maxLot.Value)}
  mt5 @args
}
function ping { param([string]$p=$PF,[int]$t=$TO,[ValidateSet('text','json')][string]$o='text') mt5 health -p $p -o $o --timeout-ms $t }
function st   { param([int]$sec=10,[string]$s=$SYM,[string]$p=$PF,[int]$t=$TO) mt5 stream -p $p --seconds $sec -s $s --timeout-ms $t }
# ====== end ======
