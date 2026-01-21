// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: https://codemirror.net/5/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode("sql", function(config, parserConfig) {
  var client         = parserConfig.client || {},
      atoms          = parserConfig.atoms || {"false": true, "true": true, "null": true},
      builtin        = parserConfig.builtin || set(defaultBuiltin),
      keywords       = parserConfig.keywords || set(sqlKeywords),
      functions      = parserConfig.functions || set(sqlFunctions),
      operatorChars  = parserConfig.operatorChars || /^[*+\-%<>!=&|~^\/]/,
      support        = parserConfig.support || {},
      hooks          = parserConfig.hooks || {},
      dateSQL        = parserConfig.dateSQL || {"date" : true, "time" : true, "timestamp" : true},
      backslashStringEscapes = parserConfig.backslashStringEscapes !== false,
      brackets       = parserConfig.brackets || /^[\{}\(\)\[\]]/,
      punctuation    = parserConfig.punctuation || /^[;.,:]/

  function tokenBase(stream, state) {
    var ch = stream.next();

    // call hooks from the mime type
    if (hooks[ch]) {
      var result = hooks[ch](stream, state);
      if (result !== false) return result;
    }

    if (support.hexNumber &&
      ((ch == "0" && stream.match(/^[xX][0-9a-fA-F]+/))
      || (ch == "x" || ch == "X") && stream.match(/^'[0-9a-fA-F]*'/))) {
      // hex
      // ref: https://dev.mysql.com/doc/refman/8.0/en/hexadecimal-literals.html
      return "number";
    } else if (support.binaryNumber &&
      (((ch == "b" || ch == "B") && stream.match(/^'[01]*'/))
      || (ch == "0" && stream.match(/^b[01]+/)))) {
      // bitstring
      // ref: https://dev.mysql.com/doc/refman/8.0/en/bit-value-literals.html
      return "number";
    } else if (ch.charCodeAt(0) > 47 && ch.charCodeAt(0) < 58) {
      // numbers
      // ref: https://dev.mysql.com/doc/refman/8.0/en/number-literals.html
      stream.match(/^[0-9]*(\.[0-9]+)?([eE][-+]?[0-9]+)?/);
      support.decimallessFloat && stream.match(/^\.(?!\.)/);
      return "number";
    } else if (ch == "?" && (stream.eatSpace() || stream.eol() || stream.eat(";"))) {
      // placeholders
      return "variable-3";
    } else if (ch == "'" || (ch == '"' && support.doubleQuote)) {
      // strings
      // ref: https://dev.mysql.com/doc/refman/8.0/en/string-literals.html
      state.tokenize = tokenLiteral(ch);
      return state.tokenize(stream, state);
    } else if ((((support.nCharCast && (ch == "n" || ch == "N"))
        || (support.charsetCast && ch == "_" && stream.match(/[a-z][a-z0-9]*/i)))
        && (stream.peek() == "'" || stream.peek() == '"'))) {
      // charset casting: _utf8'str', N'str', n'str'
      // ref: https://dev.mysql.com/doc/refman/8.0/en/string-literals.html
      return "keyword";
    } else if (support.escapeConstant && (ch == "e" || ch == "E")
        && (stream.peek() == "'" || (stream.peek() == '"' && support.doubleQuote))) {
      // escape constant: E'str', e'str'
      // ref: https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-STRINGS-ESCAPE
      state.tokenize = function(stream, state) {
        return (state.tokenize = tokenLiteral(stream.next(), true))(stream, state);
      }
      return "keyword";
    } else if (support.commentSlashSlash && ch == "/" && stream.eat("/")) {
      // 1-line comment
      stream.skipToEnd();
      return "comment";
    } else if ((support.commentHash && ch == "#")
        || (ch == "-" && stream.eat("-") && (!support.commentSpaceRequired || stream.eat(" ")))) {
      // 1-line comments
      // ref: https://kb.askmonty.org/en/comment-syntax/
      stream.skipToEnd();
      return "comment";
    } else if (ch == "/" && stream.eat("*")) {
      // multi-line comments
      // ref: https://kb.askmonty.org/en/comment-syntax/
      state.tokenize = tokenComment(1);
      return state.tokenize(stream, state);
    } else if (ch == ".") {
      // .1 for 0.1
      if (support.zerolessFloat && stream.match(/^(?:\d+(?:e[+-]?\d+)?)/i))
        return "number";
      if (stream.match(/^\.+/))
        return null
      if (stream.match(/^[\w\d_$#]+/))
        return "variable-2";
    } else if (operatorChars.test(ch)) {
      // operators
      stream.eatWhile(operatorChars);
      return "operator";
    } else if (brackets.test(ch)) {
      // brackets
      return "bracket";
    } else if (punctuation.test(ch)) {
      // punctuation
      stream.eatWhile(punctuation);
      return "punctuation";
    } else if (ch == '{' &&
        (stream.match(/^( )*(d|D|t|T|ts|TS)( )*'[^']*'( )*}/) || stream.match(/^( )*(d|D|t|T|ts|TS)( )*"[^"]*"( )*}/))) {
      // dates (weird ODBC syntax)
      // ref: https://dev.mysql.com/doc/refman/8.0/en/date-and-time-literals.html
      return "number";
    } else {
      stream.eatWhile(/^[_\w\d]/);
      var word = stream.current().toLowerCase();
      // dates (standard SQL syntax)
      // ref: https://dev.mysql.com/doc/refman/8.0/en/date-and-time-literals.html
      if (dateSQL.hasOwnProperty(word) && (stream.match(/^( )+'[^']*'/) || stream.match(/^( )+"[^"]*"/)))
        return "number";
      if (atoms.hasOwnProperty(word)) return "atom";
      if (builtin.hasOwnProperty(word)) return "type";
      if (keywords.hasOwnProperty(word)) return "keyword";
      if (functions.hasOwnProperty(word)) return "function";
      if (client.hasOwnProperty(word)) return "builtin";
      return null;
    }
  }

  // 'string', with char specified in quote escaped by '\'
  function tokenLiteral(quote, backslashEscapes) {
    return function(stream, state) {
      var escaped = false, ch;
      while ((ch = stream.next()) != null) {
        if (ch == quote && !escaped) {
          state.tokenize = tokenBase;
          break;
        }
        escaped = (backslashStringEscapes || backslashEscapes) && !escaped && ch == "\\";
      }
      return "string";
    };
  }
  function tokenComment(depth) {
    return function(stream, state) {
      var m = stream.match(/^.*?(\/\*|\*\/)/)
      if (!m) stream.skipToEnd()
      else if (m[1] == "/*") state.tokenize = tokenComment(depth + 1)
      else if (depth > 1) state.tokenize = tokenComment(depth - 1)
      else state.tokenize = tokenBase
      return "comment"
    }
  }

  function pushContext(stream, state, type) {
    state.context = {
      prev: state.context,
      indent: stream.indentation(),
      col: stream.column(),
      type: type
    };
  }

  function popContext(state) {
    state.indent = state.context.indent;
    state.context = state.context.prev;
  }

  return {
    startState: function() {
      return {tokenize: tokenBase, context: null};
    },

    token: function(stream, state) {
      if (stream.sol()) {
        if (state.context && state.context.align == null)
          state.context.align = false;
      }
      if (state.tokenize == tokenBase && stream.eatSpace()) return null;

      var style = state.tokenize(stream, state);
      if (style == "comment") return style;

      if (state.context && state.context.align == null)
        state.context.align = true;

      var tok = stream.current();
      if (tok == "(")
        pushContext(stream, state, ")");
      else if (tok == "[")
        pushContext(stream, state, "]");
      else if (state.context && state.context.type == tok)
        popContext(state);
      return style;
    },

    indent: function(state, textAfter) {
      var cx = state.context;
      if (!cx) return CodeMirror.Pass;
      var closing = textAfter.charAt(0) == cx.type;
      if (cx.align) return cx.col + (closing ? 0 : 1);
      else return cx.indent + (closing ? 0 : config.indentUnit);
    },

    blockCommentStart: "/*",
    blockCommentEnd: "*/",
    lineComment: support.commentSlashSlash ? "//" : support.commentHash ? "#" : "--",
    closeBrackets: "()[]{}''\"\"``",
    config: parserConfig
  };
});

  // `identifier`
  function hookIdentifier(stream) {
    // MySQL/MariaDB identifiers
    // ref: https://dev.mysql.com/doc/refman/8.0/en/identifier-qualifiers.html
    var ch;
    while ((ch = stream.next()) != null) {
      if (ch == "`" && !stream.eat("`")) return "variable-2";
    }
    stream.backUp(stream.current().length - 1);
    return stream.eatWhile(/\w/) ? "variable-2" : null;
  }

  // "identifier"
  function hookIdentifierDoublequote(stream) {
    // Standard SQL /SQLite identifiers
    // ref: http://web.archive.org/web/20160813185132/http://savage.net.au/SQL/sql-99.bnf.html#delimited%20identifier
    // ref: http://sqlite.org/lang_keywords.html
    var ch;
    while ((ch = stream.next()) != null) {
      if (ch == "\"" && !stream.eat("\"")) return "variable-2";
    }
    stream.backUp(stream.current().length - 1);
    return stream.eatWhile(/\w/) ? "variable-2" : null;
  }

  // variable token
  function hookVar(stream) {
    // variables
    // @@prefix.varName @varName
    // varName can be quoted with ` or ' or "
    // ref: https://dev.mysql.com/doc/refman/8.0/en/user-variables.html
    if (stream.eat("@")) {
      stream.match('session.');
      stream.match('local.');
      stream.match('global.');
    }

    if (stream.eat("'")) {
      stream.match(/^.*'/);
      return "variable-2";
    } else if (stream.eat('"')) {
      stream.match(/^.*"/);
      return "variable-2";
    } else if (stream.eat("`")) {
      stream.match(/^.*`/);
      return "variable-2";
    } else if (stream.match(/^[0-9a-zA-Z$\.\_]+/)) {
      return "variable-2";
    }
    return null;
  };

  // short client keyword token
  function hookClient(stream) {
    // \N means NULL
    // ref: https://dev.mysql.com/doc/refman/8.0/en/null-values.html
    if (stream.eat("N")) {
        return "atom";
    }
    // \g, etc
    // ref: https://dev.mysql.com/doc/refman/8.0/en/mysql-commands.html
    return stream.match(/^[a-zA-Z.#!?]/) ? "variable-2" : null;
  }

  // these keywords are used by all SQL dialects (however, a mode can still overwrite it)
  var sqlKeywords = "alter as asc by create delete desc distinct drop from group having insert into limit on order select set table update values where ";

  var sqlFunctions = "abs cast count lower ltrim max min round row_number rtrim sin sum trim upper ";

  // turn a space-separated list into an array
  function set(str) {
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
    return obj;
  }

  var defaultBuiltin = "real"

  // A generic SQL Mode. It's not a standard, it just tries to support what is generally supported
  CodeMirror.defineMIME("text/x-sql", {
    name: "sql",
    keywords: set(sqlKeywords + "begin"),
    functions: set(sqlFunctions),
    builtin: set(defaultBuiltin),
    atoms: set("false true null unknown"),
    dateSQL: set("date time timestamp"),
    support: set("doubleQuote binaryNumber hexNumber")
  });

  CodeMirror.defineMIME("text/x-mssql", {
    name: "sql",
    client: set("$partition binary_checksum checksum connectionproperty context_info current_request_id error_line error_message error_number error_procedure error_severity error_state formatmessage get_filestream_transaction_context getansinull host_id host_name isnull isnumeric min_active_rowversion newid newsequentialid rowcount_big xact_state object_id"),
    keywords: set(sqlKeywords + "add after ansi_nulls authorization backup begin break browse bulk cascade case check checkpoint close clustered collate column commit compute constraint containstable continue current cursor database dbcc deallocate declare default deny disk distributed double dump else end errlvl escape except exec execute exit fetch file fillfactor for foreign freetext freetexttable full function go goto grant holdlock identity identity_insert identitycol if index instead intersect key kill lineno load national nocheck nocount nonclustered of off offsets open opendatasource openquery openrowset openxml option over percent persisted plan precision primary print proc procedure public quoted_identifier raiserror read readtext reconfigure references replication restore restrict return returns revoke rollback rowcount rowguidcol rule save schema setuser shutdown statistics textsize then to top tran transaction trigger truncate unique updatetext use user varying view waitfor when while with writetext"),
    functions: set(sqlFunctions + "acos approx_count_distinct ascii asin atan atn2 avg ceiling char charindex checksum_agg choose coalesce concat concat_ws convert cos cot count_big current_date current_timestamp current_timezone current_user datalength dateadd datediff datediff_big datefromparts datename datepart datetime2fromparts datetimefromparts datetimeoffsetfromparts day db_name degrees difference eomonth exp floor format getdate getutcdate grouping grouping_id ident_current ident_incr ident_seed iif isdate isnull isnumeric lag lead len log log10 month nchar newid nullif parse patindex pi power quotename radians rand replace replicate reverse scope_identity session_user sessionproperty sign smalldatetimefromparts soundex space sqrt square stdev stdevp str string_agg string_escape string_split stuff substring suser_sname sysdatetime sysdatetimeoffset system_user sysutcdatetime tan timefromparts todatetimeoffset translate try_cast try_convert tsequal user_name var varp year"),
    builtin: set("bigint binary bit char date datetime datetime2 datetimeoffset decimal float geography geometry hierarchyid image int integer money nchar ntext numeric nvarchar rowversion smalldatetime smallint smallmoney sql_variant text time timestamp tinyint uniqueidentifier varbinary varchar xml"),
    atoms: set("is not null like and or in left right between inner outer join all any some cross unpivot pivot exists"),
    operatorChars: /^[*+\-%<>!=^\&|\/]/,
    brackets: /^[\{}\(\)]/,
    punctuation: /^[;.,:/]/,
    backslashStringEscapes: false,
    dateSQL: set("date datetimeoffset datetime2 smalldatetime datetime time"),
    hooks: {
      "@":   hookVar
    }
  });

  CodeMirror.defineMIME("text/x-mysql", {
    name: "sql",
    client: set("charset clear connect edit ego exit go help nopager notee nowarning pager print prompt quit rehash source status system tee"),
    keywords: set(sqlKeywords + "accessible account action after against aggregate algorithm all always analyse analyze and any ascii asensitive at auto_increment autoextend_size avg avg_row_length backup before begin between bigint binary binlog bit blob block bool boolean both btree byte cache call cascade cascaded case catalog_name chain change changed channel char character charset check checksum cipher class_origin client close coalesce code collate collation column column_format column_name columns comment commit committed compact completion compressed compression concurrent condition connection consistent constraint constraint_catalog constraint_name constraint_schema contains context continue cpu cross cube current current_date current_time current_user cursor cursor_name data database databases datafile date datetime day day_hour day_microsecond day_minute day_second deallocate dec decimal declare default default_auth definer delay_key_write delayed des_key_file describe deterministic diagnostics directory disable discard disk distinctrow div do double dual dumpfile duplicate dynamic each else elseif enable enclosed encryption end ends engine engines enum error errors escape escaped event events every exchange exists exit expansion expire explain export extended extent_size false fast faults fetch fields file file_block_size filter first fixed float float4 float8 flush follows for force foreign format found full fulltext function general generated geometry geometrycollection get get_format global grant grants group_replication handler hash help high_priority host hosts hour hour_microsecond hour_minute hour_second identified if ignore ignore_server_ids import in index indexes infile initial_size inout insensitive insert_method install instance int int1 int2 int3 int4 int8 integer interval invoker io io_after_gtids io_before_gtids io_thread ipc is isolation issuer iterate json key key_block_size keys kill language last leading leave leaves less level linear lines linestring list load local localtime localtimestamp lock locks logfile logs long longblob longtext loop low_priority master master_auto_position master_bind master_connect_retry master_delay master_heartbeat_period master_host master_log_file master_log_pos master_password master_port master_retry_count master_server_id master_ssl master_ssl_ca master_ssl_capath master_ssl_cert master_ssl_cipher master_ssl_crl master_ssl_crlpath master_ssl_key master_ssl_verify_server_cert master_tls_version master_user match max_connections_per_hour max_queries_per_hour max_rows max_size max_statement_time max_updates_per_hour max_user_connections maxvalue medium mediumblob mediumint mediumtext memory merge message_text microsecond middleint migrate min_rows minute minute_microsecond minute_second mod mode modifies modify month multilinestring multipoint multipolygon mutex mysql_errno name names national natural nchar ndb ndbcluster never new next no no_wait no_write_to_binlog nodegroup nonblocking none not null number numeric nvarchar offset old_password one only open optimize optimizer_costs option optionally options or out outfile owner pack_keys page parse_gcol_expr parser partial partition partitioning partitions password phase plugin plugin_dir plugins point polygon port precedes precision prepare preserve prev primary privileges procedure processlist profile profiles proxy purge quarter query quick range read read_only read_write reads real rebuild recover redo_buffer_size redofile redundant references regexp relay relay_log_file relay_log_pos relay_thread relaylog release reload remove rename reorganize repair repeat repeatable replace replicate_do_db replicate_do_table replicate_ignore_db replicate_ignore_table replicate_rewrite_db replicate_wild_do_table replicate_wild_ignore_table replication require reset resignal restore restrict resume return returned_sqlstate returns reverse revoke rlike rollback rollup rotate routine row row_count row_format rows rtree savepoint schedule schema schema_name schemas second second_microsecond security sensitive separator serial serializable server session share show shutdown signal signed simple slave slow smallint snapshot socket some soname sounds source spatial specific sql sql_after_gtids sql_after_mts_gaps sql_before_gtids sql_big_result sql_buffer_result sql_cache sql_calc_found_rows sql_no_cache sql_small_result sql_thread sql_tsi_day sql_tsi_hour sql_tsi_minute sql_tsi_month sql_tsi_quarter sql_tsi_second sql_tsi_week sql_tsi_year sqlexception sqlstate sqlwarning ssl stacked start starting starts stats_auto_recalc stats_persistent stats_sample_pages status stop storage stored straight_join string subclass_origin subject subpartition subpartitions super suspend swaps switches table_checksum table_name tables tablespace temporary temptable terminated text than then time timestamp timestampadd timestampdiff tinyblob tinyint tinytext to trailing transaction trigger triggers true truncate type types uncommitted undefined undo undo_buffer_size undofile unicode uninstall unique unknown unlock unsigned until upgrade usage use use_frm user user_resources using utc_date utc_time utc_timestamp validation value varbinary varchar varcharacter variables varying view virtual wait warnings week weight_string when while with without work wrapper write x509 xa xid xml xor year year_month zerofill"),
    functions: set(sqlFunctions + "acos adddate addtime asin atan atan2 bin ceil ceiling char_length character_length concat concat_ws connection_id conv cos cot curdate current_timestamp curtime date_add date_format date_sub datediff dayname dayofmonth dayofweek dayofyear degrees encrypt exp extract field find_in_set floor from_days greatest group_concat ifnull instr isnull last_day last_insert_id lcase least length ln locate log log10 log2 lpad makedate maketime md5 mid monthname now nullif period_add period_diff pi position pow power radians rand rpad sec_to_time session_user sign space sqrt std stddev ‌stddev_pop‌‌ ‌stddev_samp‌‌ str_to_date strcmp subdate substr substring substring_index subtime sysdate system_user tan time_format time_to_sec timediff to_days ucase uuid var_pop var_samp version weekday weekofyear yearweek"),
    builtin: set("bigint binary bit blob bool boolean char date datetime decimal double enum float geomcollection geometry int json linestring longblob longtext mediumblob mediumint mediumtext multilinestring multipoint multipolygon numeric point polygon set smallint text time timestamp tinyblob tinyint tinytext varbinary varchar year"),
    atoms: set("false true null unknown"),
    operatorChars: /^[*+\-%<>!=&|^]/,
    dateSQL: set("date time timestamp"),
    support: set("decimallessFloat zerolessFloat binaryNumber hexNumber doubleQuote nCharCast charsetCast commentHash commentSpaceRequired"),
    hooks: {
      "@":   hookVar,
      "`":   hookIdentifier,
      "\\":  hookClient
    }
  });

  CodeMirror.defineMIME("text/x-mariadb", {
    name: "sql",
    client: set("charset clear connect edit ego exit go help nopager notee nowarning pager print prompt quit rehash source status system tee"),
    keywords: set(sqlKeywords + "accessible action add after algorithm all always analyze asensitive at authors auto_increment autocommit avg avg_row_length before binary binlog both btree cache call cascade cascaded case catalog_name chain change changed character check checkpoint checksum class_origin client_statistics close coalesce code collate collation collations column columns comment commit committed completion concurrent condition connection consistent constraint contains continue contributors convert cross current current_date current_time current_timestamp current_user cursor data database databases day_hour day_microsecond day_minute day_second deallocate dec declare default delay_key_write delayed delimiter des_key_file describe deterministic dev_pop dev_samp deviance diagnostics directory disable discard distinctrow div dual dumpfile each elseif enable enclosed end ends engine engines enum errors escape escaped even event events every execute exists exit explain extended fast fetch field fields first flush for force foreign found_rows full fulltext function general generated get global grant grants group group_concat handler hard hash help high_priority hosts hour_microsecond hour_minute hour_second if ignore ignore_server_ids import index index_statistics infile inner innodb inout insensitive insert_method install interval invoker isolation iterate key keys kill language last leading leave left level limit linear lines list load local localtime localtimestamp lock logs low_priority master master_heartbeat_period master_ssl_verify_server_cert masters match max max_rows maxvalue message_text middleint migrate min min_rows minute_microsecond minute_second mod mode modifies modify mutex mysql_errno natural next no no_write_to_binlog offline offset one online open optimize option optionally out outer outfile pack_keys parser partition partitions password persistent phase plugin plugins prepare preserve prev primary privileges procedure processlist profile profiles purge query quick range read read_write reads real rebuild recover references regexp relaylog release remove rename reorganize repair repeatable replace require resignal restrict resume return returns revoke right rlike rollback rollup row row_format rtree savepoint schedule schema schema_name schemas second_microsecond security sensitive separator serializable server session share show shutdown signal slave slow smallint snapshot soft soname spatial specific sql sql_big_result sql_buffer_result sql_cache sql_calc_found_rows sql_no_cache sql_small_result sqlexception sqlstate sqlwarning ssl start starting starts status std stddev stddev_pop stddev_samp storage straight_join subclass_origin sum suspend table_name table_statistics tables tablespace temporary terminated to trailing transaction trigger triggers truncate uncommitted undo uninstall unique unlock upgrade usage use use_frm user user_resources user_statistics using utc_date utc_time utc_timestamp value variables varying view views virtual warnings when while with work write xa xor year_month zerofill begin do then else loop repeat"),
    builtin: set("bool boolean bit blob decimal double float long longblob longtext medium mediumblob mediumint mediumtext time timestamp tinyblob tinyint tinytext text bigint int int1 int2 int3 int4 int8 integer float float4 float8 double char varbinary varchar varcharacter precision date datetime year unsigned signed numeric"),
    atoms: set("false true null unknown"),
    operatorChars: /^[*+\-%<>!=&|^]/,
    dateSQL: set("date time timestamp"),
    support: set("decimallessFloat zerolessFloat binaryNumber hexNumber doubleQuote nCharCast charsetCast commentHash commentSpaceRequired"),
    hooks: {
      "@":   hookVar,
      "`":   hookIdentifier,
      "\\":  hookClient
    }
  });

  // provided by the phpLiteAdmin project - phpliteadmin.org
  CodeMirror.defineMIME("text/x-sqlite", {
    name: "sql",
    // commands of the official SQLite client, ref: https://www.sqlite.org/cli.html#dotcmd
    client: set("auth backup bail binary changes check clone databases dbinfo dump echo eqp exit explain fullschema headers help import imposter indexes iotrace limit lint load log mode nullvalue once open output print prompt quit read restore save scanstats schema separator session shell show stats system tables testcase timeout timer trace vfsinfo vfslist vfsname width"),
    // ref: http://sqlite.org/lang_keywords.html
    keywords: set(sqlKeywords + "abort action add after all always analyze attach autoincrement before between cascade case check collate column conflict constraint cross current current_date current_time current_timestamp database default deferrable deferred detach do each else escape except exclude exclusive exists explain fail filter first following for foreign full generated groups if ignore immediate in index indexed initially instead intersect is isnull key last match materialized natural no not nothing notnull nulls of offset others over partition plan pragma preceding primary query raise range recursive references regexp reindex release rename restrict returning row rows savepoint temp temporary then ties to trigger truncate unbounded unique using vacuum view virtual when window with without"),
    functions: set(sqlFunctions + "avg ceiling changes char coalesce date datetime format glob hex ifnull iif instr last_insert_rowid length likelihood likely ln nullif printf quote random randomblob replace soundex sqlite_compileoption_get sqlite_compileoption_used sqlite_offset sqlite_source_id sqlite_version strftime substr substring total_changes typeof unicode unlikely zeroblob"),
    // SQLite is weakly typed, ref: http://sqlite.org/datatype3.html. This is just a list of some common types.
    builtin: set("blob integer numeric text"),
    // ref: http://sqlite.org/syntax/literal-value.html
    atoms: set("null current_date current_time current_timestamp"),
    // ref: http://sqlite.org/lang_expr.html#binaryops
    operatorChars: /^[*+\-%<>!=&|/~]/,
    // SQLite is weakly typed, ref: http://sqlite.org/datatype3.html. This is just a list of some common types.
    dateSQL: set("date time timestamp datetime"),
    support: set("decimallessFloat zerolessFloat"),
    identifierQuote: "\"",  //ref: http://sqlite.org/lang_keywords.html
    hooks: {
      // bind-parameters ref:http://sqlite.org/lang_expr.html#varparam
      "@":   hookVar,
      ":":   hookVar,
      "?":   hookVar,
      "$":   hookVar,
      // The preferred way to escape Identifiers is using double quotes, ref: http://sqlite.org/lang_keywords.html
      "\"":   hookIdentifierDoublequote,
      // there is also support for backticks, ref: http://sqlite.org/lang_keywords.html
      "`":   hookIdentifier
    }
  });

  // the query language used by Apache Cassandra is called CQL, but this mime type
  // is called Cassandra to avoid confusion with Contextual Query Language
  CodeMirror.defineMIME("text/x-cassandra", {
    name: "sql",
    client: { },
    keywords: set("add all allow alter and any apply as asc authorize batch begin by clustering columnfamily compact consistency count create custom delete desc distinct drop each_quorum exists filtering from grant if in index insert into key keyspace keyspaces level limit local_one local_quorum modify nan norecursive nosuperuser not of on one order password permission permissions primary quorum rename revoke schema select set storage superuser table three to token truncate ttl two type unlogged update use user users using values where with writetime"),
    builtin: set("ascii bigint blob boolean counter decimal double float frozen inet int list map static text timestamp timeuuid tuple uuid varchar varint"),
    atoms: set("false true infinity NaN"),
    operatorChars: /^[<>=]/,
    dateSQL: { },
    support: set("commentSlashSlash decimallessFloat"),
    hooks: { }
  });

  // this is based on Peter Raganitsch's 'plsql' mode
  CodeMirror.defineMIME("text/x-plsql", {
    name:       "sql",
    client:     set("appinfo arraysize autocommit autoprint autorecovery autotrace blockterminator break btitle cmdsep colsep compatibility compute concat copycommit copytypecheck define describe echo editfile embedded escape exec execute feedback flagger flush heading headsep instance linesize lno loboffset logsource long longchunksize markup native newpage numformat numwidth pagesize pause pno recsep recsepchar release repfooter repheader serveroutput shiftinout show showmode size spool sqlblanklines sqlcase sqlcode sqlcontinue sqlnumber sqlpluscompatibility sqlprefix sqlprompt sqlterminator suffix tab term termout time timing trimout trimspool ttitle underline verify version wrap"),
    keywords: set(sqlKeywords + "add after agent aggregate all any array at attribute authid before begin between binary block body both bulk byte call calling cascade case char char_base character charset check close cluster clusters colauth collect columns comment commit committed compress connect constant constructor context crash cross current cursor dangling data date day decimal declare default deterministic double each element else elsif empty end escape except exception exceptions exclusive execute exists exit external fetch final float for forall force form full function goto grant hash heap hidden high hour identified if immediate in including index indexes indicator indices infinite instantiable instead of int intersect interval invalidate is isolation java language leading length less level library like2 like4 likec limited local lock long loop map member merge minus minute mod mode modify month multiset name nan national native nchar new nocompress nocopy not nowait number_base object of only open operator option organization others out overriding package parallel_enable parameters partition pipe pipelined pragma precision prior private procedure public raise range raw read record ref reference remainder rename replace resource result return returning reverse revoke rollback row sample save savepoint second segment self separate sequence serializable share size some sql sqlcode sqlstate standard start static stored string submultiset subpartition substitutable subtype synonym tabauth tablespace than the then time timestamp to trailing transaction trigger truncate trusted type under unique use using variable variance varray varying view views when while with work write year zone"),
    functions: set(sqlFunctions + "acos add_months ascii asciistr asin atan atan2 avg bfilename bin_to_num bitand cardinality ceil chartorowid chr coalesce compose concat convert corr cos cosh covar_pop covar_samp cume_dist current_date current_timestamp dbtimezone decode decompose dense_rank dump empty_blob empty_clob exp extract first_value floor from_tz greatest group_id hextoraw initcap instr instr2 instr4 instrb instrc last_day last_value lead least length2 length4 lengthb lengthc listagg ln lnnvl localtimestamp log lpad median months_between nanvl nchr new_time next_day nth_value nullif numtodsinterval numtoyminterval nvl nvl2 ora_database_name power rank rawtohex regexp_count regexp_instr regexp_replace regexp_substr rownum rpad sessiontimezone sign sinh soundex sqrt stddev stddev_pop stddev_samp substr sys_context sys_extract_utc sys_guid sysdate systimestamp tan tanh to_char to_clob to_date to_dsinterval to_lob to_multi_byte to_nclob to_number to_single_byte to_timestamp to_timestamp_tz to_yminterval translate trunc tz_offset uid user userenv var_pop var_samp vsize"),
    builtin: set("bfile binary_double binary_float blob char clob date decimal float int integer long nchar nclob number nvarchar2 raw sdo_geometry smallint st_geometry timestamp varchar varchar2"),
    operatorChars: /^[*\/+\-%<>!=~]/,
    dateSQL:    set("date time timestamp"),
    support:    set("doubleQuote nCharCast zerolessFloat binaryNumber hexNumber")
  });

  // Created to support specific hive keywords
  CodeMirror.defineMIME("text/x-hive", {
    name: "sql",
    keywords: set("select alter $elem$ $key$ $value$ add after all analyze and archive as asc before between binary both bucket buckets by cascade case cast change cluster clustered clusterstatus collection column columns comment compute concatenate continue create cross cursor data database databases dbproperties deferred delete delimited desc describe directory disable distinct distribute drop else enable end escaped exclusive exists explain export extended external fetch fields fileformat first format formatted from full function functions grant group having hold_ddltime idxproperties if import in index indexes inpath inputdriver inputformat insert intersect into is items join keys lateral left like limit lines load local location lock locks mapjoin materialized minus msck no_drop nocompress not of offline on option or order out outer outputdriver outputformat overwrite partition partitioned partitions percent plus preserve procedure purge range rcfile read readonly reads rebuild recordreader recordwriter recover reduce regexp rename repair replace restrict revoke right rlike row schema schemas semi sequencefile serde serdeproperties set shared show show_database sort sorted ssl statistics stored streamtable table tables tablesample tblproperties temporary terminated textfile then tmp to touch transform trigger unarchive undo union uniquejoin unlock update use using utc utc_tmestamp view when where while with admin authorization char compact compactions conf cube current current_date current_timestamp day decimal defined dependency directories elem_type exchange file following for grouping hour ignore inner interval jar less logical macro minute month more none noscan over owner partialscan preceding pretty principals protection reload rewrite role roles rollup rows second server sets skewed transactions truncate unbounded unset uri user values window year"),
    builtin: set("bool boolean long timestamp tinyint smallint bigint int float double date datetime unsigned string array struct map uniontype key_type utctimestamp value_type varchar"),
    atoms: set("false true null unknown"),
    operatorChars: /^[*+\-%<>!=]/,
    dateSQL: set("date timestamp"),
    support: set("doubleQuote binaryNumber hexNumber")
  });

  CodeMirror.defineMIME("text/x-pgsql", {
    name: "sql",
    client: set("source"),
    // For PostgreSQL - https://www.postgresql.org/docs/11/sql-keywords-appendix.html
    // For pl/pgsql lang - https://github.com/postgres/postgres/blob/REL_11_2/src/pl/plpgsql/src/pl_scanner.c
    keywords: set(sqlKeywords + "abort abs absent absolute access according acos action ada add admin after aggregate all allocate also always analyse analyze and any are array array_agg array_max_cardinality as asensitive asin assertion assignment asymmetric at atan atomic attach attribute attributes authorization backward base64 before begin begin_frame begin_partition bernoulli between bigint binary bit bit_length blob blocked bom boolean both breadth cache call called cardinality cascade cascaded case cast catalog catalog_name ceil ceiling chain chaining char char_length character character_length character_set_catalog character_set_name character_set_schema characteristics characters check checkpoint class class_origin classifier clob close cluster coalesce cobol collate collation collation_catalog collation_name collation_schema collect column column_name columns command_function command_function_code comment comments commit committed compression concurrently condition condition_number conditional configuration conflict connect connection connection_name constraint constraint_catalog constraint_name constraint_schema constraints constructor contains content continue control conversion copy corr corresponding cos cosh cost covar_pop covar_samp cross csv cube cume_dist current current_catalog current_date current_default_transform_group current_path current_role current_row current_schema current_time current_timestamp current_transform_group_for_type current_user cursor cursor_name cycle data database datalink date datetime_interval_code datetime_interval_precision day deallocate dec decfloat decimal declare default defaults deferrable deferred define defined definer degree delimiter delimiters dense_rank depends depth deref derived describe descriptor detach deterministic diagnostics dictionary disable discard disconnect dispatch dlnewcopy dlpreviouscopy dlurlcomplete dlurlcompleteonly dlurlcompletewrite dlurlpath dlurlpathonly dlurlpathwrite dlurlscheme dlurlserver dlvalue do document domain double dynamic dynamic_function dynamic_function_code each element else empty enable encoding encrypted end end_frame end_partition end-exec enforced enum equals error escape event every except exception exclude excluding exclusive exec execute exists exp explain expression extension external extract false family fetch file filter final finalize finish first first_value flag float floor following for force foreign format fortran forward found frame_row free freeze fs fulfill full function functions fusion general generated get global go goto grant granted greatest grouping groups handler header hex hierarchy hold hour identity if ignore ilike immediate immediately immutable implementation implicit import in include including increment indent index indexes indicator inherit inherits initial initially inline inner inout input insensitive instance instantiable instead int integer integrity intersect intersection interval invoker is isnull isolation join json json_array json_arrayagg json_exists json_object json_objectagg json_query json_table json_table_primitive json_value keep key key_member key_type keys label lag language large last last_value lateral lead leading leakproof least left length level library like like_regex link listagg listen ln load local localtime localtimestamp location locator lock locked log log10 logged lower map mapping match match_number match_recognize matched matches materialized maxvalue measures member merge message_length message_octet_length message_text method minute minvalue mod mode modifies module month more move multiset mumps names namespace national natural nchar nclob nested nesting new next nfc nfd nfkc nfkd nil no none normalize normalized not nothing notify notnull nowait nth_value ntile nullable nullif nulls number numeric object occurrences_regex octet_length octets of off offset oids old omit one only open operator option options or ordering ordinality others out outer output over overflow overlaps overlay overriding owned owner pad parallel parameter parameter_mode parameter_name parameter_ordinal_position parameter_specific_catalog parameter_specific_name parameter_specific_schema parser partial partition pascal pass passing passthrough password past path pattern per percent percent_rank percentile_cont percentile_disc period permission permute placing plan plans pli policy portion position position_regex power precedes preceding precision prepare prepared preserve primary prior private privileges procedural procedure procedures program prune ptf public publication quote quotes range rank read reads real reassign recheck recovery recursive ref references referencing refresh regr_avgx regr_avgy regr_count regr_intercept regr_r2 regr_slope regr_sxx regr_sxy regr_syy reindex relative release rename repeatable replace replica requiring reset respect restart restore restrict result return returned_cardinality returned_length returned_octet_length returned_sqlstate returning returns revoke right role rollback rollup routine routine_catalog routine_name routine_schema routines row row_count row_number rows rule running savepoint scalar scale schema schema_name schemas scope scope_catalog scope_name scope_schema scroll search second section security seek selective self sensitive sequence sequences serializable server server_name session session_user setof sets share show similar simple sin sinh size skip smallint snapshot some source space specific specific_name specifictype sql sqlcode sqlerror sqlexception sqlstate sqlwarning sqrt stable standalone start state statement static statistics stdin stdout storage stored strict string strip structure style subclass_origin submultiset subscription subset substring substring_regex succeeds support symmetric sysid system system_time system_user table_name tables tablesample tablespace tan tanh temp template temporary text then through ties time timestamp timezone_hour timezone_minute to token top_level_count trailing transaction transaction_active transactions_committed transactions_rolled_back transform transforms translate translate_regex translation treat trigger trigger_catalog trigger_name trigger_schema trim trim_array true truncate trusted type types uescape unbounded uncommitted unconditional under unencrypted union unique unknown unlink unlisten unlogged unmatched unnamed unnest until untyped upper uri usage user user_defined_type_catalog user_defined_type_code user_defined_type_name user_defined_type_schema using utf16 utf32 utf8 vacuum valid validate validator value value_of varbinary varchar variadic varying verbose version versioning view views volatile when whenever whitespace width_bucket window with within without work wrapper write xml xmlagg xmlattributes xmlbinary xmlcast xmlcomment xmlconcat xmldeclaration xmldocument xmlelement xmlexists xmlforest xmliterate xmlnamespaces xmlparse xmlpi xmlquery xmlroot xmlschema xmlserialize xmltable xmltext xmlvalidate year yes zone"),
    functions: set(sqlFunctions +"acos acosd ascii asin asind atan atan2 atan2d avg btrim cbrt ceil ceiling char_length character_length chr coalesce concat concat_ws convert cos cosd cot cotd current_database current_schema current_timestamp current_user date_part degrees exp extract factorial floor format gcd gen_random_uuid initcap lastval lcm left length ln log log10 lpad md5 min_scale mod normalize octet_length overlay pi position power radians random repeat replace reverse right rpad scale sign sind sqrt starts_with stddev stddev_pop stddev_samp string_agg strpos substr substring tan tand to_char to_date to_number to_timestamp trim_scale trunc var_pop var_samp"),
    // https://www.postgresql.org/docs/11/datatype.html
    builtin: set("bigint bigserial bit boolean box bytea char character cidr circle cstring date datemultirange daterange geography geometry gtsvector inet int2vector int4multirange int4range int8range integer interval json jsonb jsonpath line lseg macaddr macaddr8 money name numeric nummultirange numrange oid oidvector path pg_brin_bloom_summary pg_brin_minmax_multi_summary pg_dependencies pg_lsn pg_mcv_list pg_ndistinct pg_node_tree pg_snapshot point polygon refcursor regclass regcollation regconfig regdictionary regnamespace regoper regoperator regproc regprocedure regrole regtype serial smallint smallserial text tid tsmultirange tsquery tsrange tstzmultirange tstzrange tsvector txid_snapshot uuid xid xid8 xml"),
    atoms: set("false true null unknown"),
    operatorChars: /^[*\/+\-%<>!=&|^\/#@?~]/,
    backslashStringEscapes: false,
    identifierQuote: "\"", // https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-IDENTIFIERS
    hooks: {
      "\"":   hookIdentifierDoublequote
    },
    dateSQL: set("date time timestamp"),
    support: set("decimallessFloat zerolessFloat binaryNumber hexNumber nCharCast charsetCast escapeConstant")
  });

  // Google's SQL-like query language, GQL
  CodeMirror.defineMIME("text/x-gql", {
    name: "sql",
    keywords: set("ancestor and asc by contains desc descendant distinct from group has in is limit offset on order select superset where"),
    atoms: set("false true"),
    builtin: set("blob datetime first key __key__ string integer double boolean null"),
    operatorChars: /^[*+\-%<>!=]/
  });

  // Greenplum
  CodeMirror.defineMIME("text/x-gpsql", {
    name: "sql",
    client: set("source"),
    //https://github.com/greenplum-db/gpdb/blob/master/src/include/parser/kwlist.h
    keywords: set("abort absolute access action active add admin after aggregate all also alter always analyse analyze and any array as asc assertion assignment asymmetric at authorization backward before begin between bigint binary bit boolean both by cache called cascade cascaded case cast chain char character characteristics check checkpoint class close cluster coalesce codegen collate column comment commit committed concurrency concurrently configuration connection constraint constraints contains content continue conversion copy cost cpu_rate_limit create createdb createexttable createrole createuser cross csv cube current current_catalog current_date current_role current_schema current_time current_timestamp current_user cursor cycle data database day deallocate dec decimal declare decode default defaults deferrable deferred definer delete delimiter delimiters deny desc dictionary disable discard distinct distributed do document domain double drop dxl each else enable encoding encrypted end enum errors escape every except exchange exclude excluding exclusive execute exists explain extension external extract false family fetch fields filespace fill filter first float following for force foreign format forward freeze from full function global grant granted greatest group group_id grouping handler hash having header hold host hour identity if ignore ilike immediate immutable implicit in including inclusive increment index indexes inherit inherits initially inline inner inout input insensitive insert instead int integer intersect interval into invoker is isnull isolation join key language large last leading least left level like limit list listen load local localtime localtimestamp location lock log login mapping master match maxvalue median merge minute minvalue missing mode modifies modify month move name names national natural nchar new newline next no nocreatedb nocreateexttable nocreaterole nocreateuser noinherit nologin none noovercommit nosuperuser not nothing notify notnull nowait null nullif nulls numeric object of off offset oids old on only operator option options or order ordered others out outer over overcommit overlaps overlay owned owner parser partial partition partitions passing password percent percentile_cont percentile_disc placing plans position preceding precision prepare prepared preserve primary prior privileges procedural procedure protocol queue quote randomly range read readable reads real reassign recheck recursive ref references reindex reject relative release rename repeatable replace replica reset resource restart restrict returning returns revoke right role rollback rollup rootpartition row rows rule savepoint scatter schema scroll search second security segment select sequence serializable session session_user set setof sets share show similar simple smallint some split sql stable standalone start statement statistics stdin stdout storage strict strip subpartition subpartitions substring superuser symmetric sysid system table tablespace temp template temporary text then threshold ties time timestamp to trailing transaction treat trigger trim true truncate trusted type unbounded uncommitted unencrypted union unique unknown unlisten until update user using vacuum valid validation validator value values varchar variadic varying verbose version view volatile web when where whitespace window with within without work writable write xml xmlattributes xmlconcat xmlelement xmlexists xmlforest xmlparse xmlpi xmlroot xmlserialize year yes zone"),
    builtin: set("bigint int8 bigserial serial8 bit varying varbit boolean bool box bytea character char varchar cidr circle date double precision float float8 inet integer int int4 interval json jsonb line lseg macaddr macaddr8 money numeric decimal path pg_lsn point polygon real float4 smallint int2 smallserial serial2 serial serial4 text time without zone with timetz timestamp timestamptz tsquery tsvector txid_snapshot uuid xml"),
    atoms: set("false true null unknown"),
    operatorChars: /^[*+\-%<>!=&|^\/#@?~]/,
    dateSQL: set("date time timestamp"),
    support: set("decimallessFloat zerolessFloat binaryNumber hexNumber nCharCast charsetCast")
  });

  // Spark SQL
  CodeMirror.defineMIME("text/x-sparksql", {
    name: "sql",
    keywords: set("add after all alter analyze and anti archive array as asc at between bucket buckets by cache cascade case cast change clear cluster clustered codegen collection column columns comment commit compact compactions compute concatenate cost create cross cube current current_date current_timestamp database databases data dbproperties defined delete delimited deny desc describe dfs directories distinct distribute drop else end escaped except exchange exists explain export extended external false fields fileformat first following for format formatted from full function functions global grant group grouping having if ignore import in index indexes inner inpath inputformat insert intersect interval into is items join keys last lateral lazy left like limit lines list load local location lock locks logical macro map minus msck natural no not null nulls of on optimize option options or order out outer outputformat over overwrite partition partitioned partitions percent preceding principals purge range recordreader recordwriter recover reduce refresh regexp rename repair replace reset restrict revoke right rlike role roles rollback rollup row rows schema schemas select semi separated serde serdeproperties set sets show skewed sort sorted start statistics stored stratify struct table tables tablesample tblproperties temp temporary terminated then to touch transaction transactions transform true truncate unarchive unbounded uncache union unlock unset use using values view when where window with"),
    builtin: set("abs acos acosh add_months aggregate and any approx_count_distinct approx_percentile array array_contains array_distinct array_except array_intersect array_join array_max array_min array_position array_remove array_repeat array_sort array_union arrays_overlap arrays_zip ascii asin asinh assert_true atan atan2 atanh avg base64 between bigint bin binary bit_and bit_count bit_get bit_length bit_or bit_xor bool_and bool_or boolean bround btrim cardinality case cast cbrt ceil ceiling char char_length character_length chr coalesce collect_list collect_set concat concat_ws conv corr cos cosh cot count count_if count_min_sketch covar_pop covar_samp crc32 cume_dist current_catalog current_database current_date current_timestamp current_timezone current_user date date_add date_format date_from_unix_date date_part date_sub date_trunc datediff day dayofmonth dayofweek dayofyear decimal decode degrees delimited dense_rank div double e element_at elt encode every exists exp explode explode_outer expm1 extract factorial filter find_in_set first first_value flatten float floor forall format_number format_string from_csv from_json from_unixtime from_utc_timestamp get_json_object getbit greatest grouping grouping_id hash hex hour hypot if ifnull in initcap inline inline_outer input_file_block_length input_file_block_start input_file_name inputformat instr int isnan isnotnull isnull java_method json_array_length json_object_keys json_tuple kurtosis lag last last_day last_value lcase lead least left length levenshtein like ln locate log log10 log1p log2 lower lpad ltrim make_date make_dt_interval make_interval make_timestamp make_ym_interval map map_concat map_entries map_filter map_from_arrays map_from_entries map_keys map_values map_zip_with max max_by md5 mean min min_by minute mod monotonically_increasing_id month months_between named_struct nanvl negative next_day not now nth_value ntile nullif nvl nvl2 octet_length or outputformat overlay parse_url percent_rank percentile percentile_approx pi pmod posexplode posexplode_outer position positive pow power printf quarter radians raise_error rand randn random rank rcfile reflect regexp regexp_extract regexp_extract_all regexp_like regexp_replace repeat replace reverse right rint rlike round row_number rpad rtrim schema_of_csv schema_of_json second sentences sequence sequencefile serde session_window sha sha1 sha2 shiftleft shiftright shiftrightunsigned shuffle sign signum sin sinh size skewness slice smallint some sort_array soundex space spark_partition_id split sqrt stack std stddev stddev_pop stddev_samp str_to_map string struct substr substring substring_index sum tan tanh textfile timestamp timestamp_micros timestamp_millis timestamp_seconds tinyint to_csv to_date to_json to_timestamp to_unix_timestamp to_utc_timestamp transform transform_keys transform_values translate trim trunc try_add try_divide typeof ucase unbase64 unhex uniontype unix_date unix_micros unix_millis unix_seconds unix_timestamp upper uuid var_pop var_samp variance version weekday weekofyear when width_bucket window xpath xpath_boolean xpath_double xpath_float xpath_int xpath_long xpath_number xpath_short xpath_string xxhash64 year zip_with"),
    atoms: set("false true null"),
    operatorChars: /^[*\/+\-%<>!=~&|^]/,
    dateSQL: set("date time timestamp"),
    support: set("doubleQuote zerolessFloat")
  });

  // Esper
  CodeMirror.defineMIME("text/x-esper", {
    name: "sql",
    client: set("source"),
    // http://www.espertech.com/esper/release-5.5.0/esper-reference/html/appendix_keywords.html
    keywords: set("alter and as asc between by count create delete desc distinct drop from group having in insert into is join like not on or order select set table union update values where limit after all and as at asc avedev avg between by case cast coalesce count create current_timestamp day days delete define desc distinct else end escape events every exists false first from full group having hour hours in inner insert instanceof into irstream is istream join last lastweekday left limit like max match_recognize matches median measures metadatasql min minute minutes msec millisecond milliseconds not null offset on or order outer output partition pattern prev prior regexp retain-union retain-intersection right rstream sec second seconds select set some snapshot sql stddev sum then true unidirectional until update variable weekday when where window"),
    builtin: {},
    atoms: set("false true null"),
    operatorChars: /^[*+\-%<>!=&|^\/#@?~]/,
    dateSQL: set("time"),
    support: set("decimallessFloat zerolessFloat binaryNumber hexNumber")
  });

  // Trino (formerly known as Presto)
  CodeMirror.defineMIME("text/x-trino", {
    name: "sql",
    // https://github.com/trinodb/trino/blob/bc7a4eeedde28684c7ae6f74cefcaf7c6e782174/core/trino-parser/src/main/antlr4/io/trino/sql/parser/SqlBase.g4#L859-L1129
    // https://github.com/trinodb/trino/blob/bc7a4eeedde28684c7ae6f74cefcaf7c6e782174/docs/src/main/sphinx/functions/list.rst
    keywords: set("abs absent acos add admin after all all_match alter analyze and any any_match approx_distinct approx_most_frequent approx_percentile approx_set arbitrary array_agg array_distinct array_except array_intersect array_join array_max array_min array_position array_remove array_sort array_union arrays_overlap as asc asin at at_timezone atan atan2 authorization avg bar bernoulli beta_cdf between bing_tile bing_tile_at bing_tile_coordinates bing_tile_polygon bing_tile_quadkey bing_tile_zoom_level bing_tiles_around bit_count bitwise_and bitwise_and_agg bitwise_left_shift bitwise_not bitwise_or bitwise_or_agg bitwise_right_shift bitwise_right_shift_arithmetic bitwise_xor bool_and bool_or both by call cardinality cascade case cast catalogs cbrt ceil ceiling char2hexint checksum chr classify coalesce codepoint column columns combinations comment commit committed concat concat_ws conditional constraint contains contains_sequence convex_hull_agg copartition corr cos cosh cosine_similarity count count_if covar_pop covar_samp crc32 create cross cube cume_dist current current_catalog current_date current_groups current_path current_role current_schema current_time current_timestamp current_timezone current_user data date_add date_diff date_format date_parse date_trunc day day_of_month day_of_week day_of_year deallocate default define definer degrees delete dense_rank deny desc describe descriptor distinct distributed dow doy drop e element_at else empty empty_approx_set encoding end error escape evaluate_classifier_predictions every except excluding execute exists exp explain extract false features fetch filter final first first_value flatten floor following for format format_datetime format_number from from_base from_base32 from_base64 from_base64url from_big_endian_32 from_big_endian_64 from_encoded_polyline from_geojson_geometry from_hex from_ieee754_32 from_ieee754_64 from_iso8601_date from_iso8601_timestamp from_iso8601_timestamp_nanos from_unixtime from_unixtime_nanos from_utf8 full functions geometric_mean geometry_from_hadoop_shape geometry_invalid_reason geometry_nearest_points geometry_to_bing_tiles geometry_union geometry_union_agg grant granted grants graphviz great_circle_distance greatest group grouping groups hamming_distance hash_counts having histogram hmac_md5 hmac_sha1 hmac_sha256 hmac_sha512 hour human_readable_seconds if ignore in including index infinity initial inner input insert intersect intersection_cardinality into inverse_beta_cdf inverse_normal_cdf invoker io is is_finite is_infinite is_json_scalar is_nan isolation jaccard_index join json_array json_array_contains json_array_get json_array_length json_exists json_extract json_extract_scalar json_format json_object json_parse json_query json_size json_value keep key keys kurtosis lag last last_day_of_month last_value lateral lead leading learn_classifier learn_libsvm_classifier learn_libsvm_regressor learn_regressor least left length level levenshtein_distance like limit line_interpolate_point line_interpolate_points line_locate_point listagg ln local localtime localtimestamp log log10 log2 logical lower lpad ltrim luhn_check make_set_digest map_agg map_concat map_entries map_filter map_from_entries map_keys map_union map_values map_zip_with match match_recognize matched matches materialized max max_by md5 measures merge merge_set_digest millisecond min min_by minute mod month multimap_agg multimap_from_entries murmur3 nan natural next nfc nfd nfkc nfkd ngrams no none none_match normal_cdf normalize not now nth_value ntile null nullif nulls numeric_histogram object objectid_timestamp of offset omit on one only option or order ordinality outer output over overflow parse_data_size parse_datetime parse_duration partition partitions passing past path pattern per percent_rank permute pi position pow power preceding prepare privileges properties prune qdigest_agg quarter quotes radians rand random range rank read recursive reduce reduce_agg refresh regexp_count regexp_extract regexp_extract_all regexp_like regexp_position regexp_replace regexp_split regr_intercept regr_slope regress rename render repeat repeatable replace reset respect restrict returning reverse revoke rgb right role roles rollback rollup round row_number rows rpad rtrim running scalar schema schemas second security seek select sequence serializable session set sets sha1 sha256 sha512 show shuffle sign simplify_geometry sin skewness skip slice some soundex spatial_partitioning spatial_partitions split split_part split_to_map split_to_multimap spooky_hash_v2_32 spooky_hash_v2_64 sqrt st_area st_asbinary st_astext st_boundary st_buffer st_centroid st_contains st_convexhull st_coorddim st_crosses st_difference st_dimension st_disjoint st_distance st_endpoint st_envelope st_envelopeaspts st_equals st_exteriorring st_geometries st_geometryfromtext st_geometryn st_geometrytype st_geomfrombinary st_interiorringn st_interiorrings st_intersection st_intersects st_isclosed st_isempty st_isring st_issimple st_isvalid st_length st_linefromtext st_linestring st_multipoint st_numgeometries st_numinteriorring st_numpoints st_overlaps st_point st_pointn st_points st_polygon st_relate st_startpoint st_symdifference st_touches st_union st_within st_x st_xmax st_xmin st_y st_ymax st_ymin start starts_with stats stddev stddev_pop stddev_samp string strpos subset substr substring sum system table tables tablesample tan tanh tdigest_agg text then ties timestamp_objectid timezone_hour timezone_minute to to_base to_base32 to_base64 to_base64url to_big_endian_32 to_big_endian_64 to_char to_date to_encoded_polyline to_geojson_geometry to_geometry to_hex to_ieee754_32 to_ieee754_64 to_iso8601 to_milliseconds to_spherical_geography to_timestamp to_unixtime to_utf8 trailing transaction transform transform_keys transform_values translate trim trim_array true truncate try try_cast type typeof uescape unbounded uncommitted unconditional union unique unknown unmatched unnest update upper url_decode url_encode url_extract_fragment url_extract_host url_extract_parameter url_extract_path url_extract_port url_extract_protocol url_extract_query use user using utf16 utf32 utf8 validate value value_at_quantile values values_at_quantiles var_pop var_samp variance verbose version view week week_of_year when where width_bucket wilson_interval_lower wilson_interval_upper window with with_timezone within without word_stem work wrapper write xxhash64 year year_of_week yow zip zip_with"),
    // https://github.com/trinodb/trino/blob/bc7a4eeedde28684c7ae6f74cefcaf7c6e782174/core/trino-main/src/main/java/io/trino/metadata/TypeRegistry.java#L131-L168
    // https://github.com/trinodb/trino/blob/bc7a4eeedde28684c7ae6f74cefcaf7c6e782174/plugin/trino-ml/src/main/java/io/trino/plugin/ml/MLPlugin.java#L35
    // https://github.com/trinodb/trino/blob/bc7a4eeedde28684c7ae6f74cefcaf7c6e782174/plugin/trino-mongodb/src/main/java/io/trino/plugin/mongodb/MongoPlugin.java#L32
    // https://github.com/trinodb/trino/blob/bc7a4eeedde28684c7ae6f74cefcaf7c6e782174/plugin/trino-geospatial/src/main/java/io/trino/plugin/geospatial/GeoPlugin.java#L37
    builtin: set("array bigint bingtile boolean char codepoints color date decimal double function geometry hyperloglog int integer interval ipaddress joniregexp json json2016 jsonpath kdbtree likepattern map model objectid p4hyperloglog precision qdigest re2jregexp real regressor row setdigest smallint sphericalgeography tdigest time timestamp tinyint uuid varbinary varchar zone"),
    atoms: set("false true null unknown"),
    // https://trino.io/docs/current/functions/list.html#id1
    operatorChars: /^[[\]|<>=!\-+*/%]/,
    dateSQL: set("date time timestamp zone"),
    // hexNumber is necessary for VARBINARY literals, e.g. X'65683F'
    // but it also enables 0xFF hex numbers, which Trino doesn't support.
    support: set("decimallessFloat zerolessFloat hexNumber")
  });
});

/*
  How Properties of Mime Types are used by SQL Mode
  =================================================

  keywords:
    A list of keywords you want to be highlighted.
  builtin:
    A list of builtin types you want to be highlighted (if you want types to be of class "builtin" instead of "keyword").
  operatorChars:
    All characters that must be handled as operators.
  client:
    Commands parsed and executed by the client (not the server).
  support:
    A list of supported syntaxes which are not common, but are supported by more than 1 DBMS.
    * zerolessFloat: .1
    * decimallessFloat: 1.
    * hexNumber: X'01AF' X'01af' x'01AF' x'01af' 0x01AF 0x01af
    * binaryNumber: b'01' B'01' 0b01
    * doubleQuote: "string"
    * escapeConstant: E''
    * nCharCast: N'string'
    * charsetCast: _utf8'string'
    * commentHash: use # char for comments
    * commentSlashSlash: use // for comments
    * commentSpaceRequired: require a space after -- for comments
  atoms:
    Keywords that must be highlighted as atoms,. Some DBMS's support more atoms than others:
    UNKNOWN, INFINITY, UNDERFLOW, NaN...
  dateSQL:
    Used for date/time SQL standard syntax, because not all DBMS's support same temporal types.
*/
