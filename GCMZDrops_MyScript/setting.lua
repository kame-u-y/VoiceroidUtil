-- このファイルの名前を `setting.lua` に変更して設定を書き足すことで、
-- PSDToolKit における一部の振る舞いを変更することが可能です。
-- 使い方の詳しい解説は付属のマニュアルを参照してください。
local P = {} -- これは消さないこと


-- この辺に設定を書く

function P:wav_examodifler_subtitle_list(exa, values, modifiers, index)
    exa:set("vo", "start", values.SUBTITLE_START_LIST[index])
    exa:set("vo", "end", values.SUBTITLE_END_LIST[index])
    exa:set("vo", "group", self.wav_subtitle_group)
    local text = values.SUBTITLE_TEXT_LIST[index]
    if self.wav_subtitle == 2 then
      text = self:wav_subtitle_scripter(text)
    end
    exa:set("vo.0", "text", modifiers.ENCODE_TEXT(text))
end

return P -- これは消さないこと
