-- ���̃t�@�C���̖��O�� `setting.lua` �ɕύX���Đݒ�������������ƂŁA
-- PSDToolKit �ɂ�����ꕔ�̐U�镑����ύX���邱�Ƃ��\�ł��B
-- �g�����̏ڂ�������͕t���̃}�j���A�����Q�Ƃ��Ă��������B
local P = {} -- ����͏����Ȃ�����


-- ���̕ӂɐݒ������

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

return P -- ����͏����Ȃ�����
