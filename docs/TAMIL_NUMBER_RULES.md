# Tamil Number Conversion Rules & Linguistic Specification

**Language**: Tamil (`ta` / தமிழ்)  
**Corpus Sources**:
1. Tamil Virtual Academy (தமிழ் இணையக் கல்விக்கழகம் - எண்களும் பெயர்களும்)
2. Open-Tamil Numeral Engine (`tamil.numeral`)
3. BSI Tamil Bible (பரிசுத்த வேதாகமம் - Old Version)
4. Indian Revised Version (IRV) Tamil Scripture Data

---

## 1. Number Range Specification

| Number Range | Direction | Input Form | Output Form | Example |
| :--- | :--- | :--- | :--- | :--- |
| **0 – 199** | **Digit $\rightarrow$ Text** | Numeric digits (Arabic: `0`–`199`) or Ordinals (`2nd`, `2-ஆம்`) | Tamil text words | `175` $\rightarrow$ `நூற்று எழுபத்தைந்து`<br>`2nd` / `2-ஆம்` $\rightarrow$ `இரண்டாம்` |
| **200 and above** | **Text $\rightarrow$ Digit** | Spelled-out Tamil number phrases | Digits with Indian thousand separator (`1,23,456,789`) | `இருநூறு` $\rightarrow$ `200`<br>`ஐயாயிரம்` $\rightarrow$ `5,000`<br>`ஒரு லட்சம்` $\rightarrow$ `1,00,000` |

---

## 2. Range 0 – 199: Digit to Tamil Text

### 2.1 Single Digits (0 – 9)

| Digit | Tamil Word (Standalone / Cardinal) | Adjectival Form (before noun) | Ordinal Form |
| :--- | :--- | :--- | :--- |
| 0 | பூஜ்யம் / பூச்சியம் | பூஜ்ய | பூஜ்யமாவது |
| 1 | ஒன்று | ஒரு / ஓர் | முதலாம் / முதலாவது / முதல் |
| 2 | இரண்டு | இரு | இரண்டாம் / இரண்டாவது |
| 3 | மூன்று | முப்ப / மு | மூன்றாம் / மூன்றாவது |
| 4 | நான்கு | நால / நா | நான்காம் / நான்காவது |
| 5 | ஐந்து | ஐ | ஐந்தாம் / ஐந்தாவது |
| 6 | ஆறு | அறு | ஆறாம் / ஆறாவது |
| 7 | ஏழு | எழு | ஏழாம் / ஏழாவது |
| 8 | எட்டு | எண் | எட்டாம் / எட்டாவது |
| 9 | ஒன்பது | ஒன்ப | ஒன்பதாம் / ஒன்பதாவது |

### 2.2 Teens (10 – 19)

| Number | Tamil Word | Ordinal Form |
| :--- | :--- | :--- |
| 10 | பத்து | பத்தாம் / பத்தாவது |
| 11 | பதினொன்று | பதினொன்றாம் / பதினொன்றாவது |
| 12 | பன்னிரண்டு | பன்னிரண்டாம் / பன்னிரண்டாவது |
| 13 | பதின்மூன்று | பதின்மூன்றாம் / பதின்மூன்றாவது |
| 14 | பதினான்கு | பதினான்காம் / பதினான்காவது |
| 15 | பதினைந்து | பதினைந்தாம் / பதினைந்தாவது |
| 16 | பதினாறு | பதினாறாம் / பதினாறாவது |
| 17 | பதினேழு | பதினேழாம் / பதினேழாவது |
| 18 | பதினெட்டு | பதினெட்டாம் / பதினெட்டாவது |
| 19 | பத்தொன்பது | பத்தொன்பதாம் / பத்தொன்பதாவது |

### 2.3 Tens (20 – 90)

| Tens | Base Form | Combining Form (Sandhi) |
| :--- | :--- | :--- |
| 20 | இருபது | இருபத்து- (e.g. இருபத்தொன்று, இருபத்து இரண்டு) |
| 30 | முப்பது | முப்பத்து- (e.g. முப்பத்தைந்து) |
| 40 | நாற்பது | நாற்பத்து- (e.g. நாற்பத்து இரண்டு) |
| 50 | ஐம்பது | ஐம்பத்து- (e.g. ஐம்பத்தாறு) |
| 60 | அறுபது | அறுபத்து- (e.g. அறுபத்தேழு) |
| 70 | எழுபது | எழுபத்து- (e.g. எழுபத்தெட்டு) |
| 80 | எண்பது | எண்பத்து- (e.g. எண்பத்தொன்பது) |
| 90 | தொண்ணூறு | தொண்ணூற்று- (e.g. தொண்ணூற்றொன்பது) |

### 2.4 Hundreds (100 – 199)

- Base: `100` = `நூறு`
- Combining Prefix: `நூற்று` (e.g., `நூற்று ஒன்று`, `நூற்று இரண்டு`, ..., `நூற்று எழுபத்தைந்து`, `நூற்று தொண்ணூற்றொன்பது`).
- Sandhi Variations supported:
  - Spaced: `நூற்று எழுபத்தைந்து` (175)
  - Euphonic liaison: `நூற்றெழுபத்தைந்து` (175)

---

## 3. Range 200+: Tamil Text to Digits

### 3.1 Hundreds Base Forms ($\ge 200$)

| Value | Tamil Word | Combining Form |
| :--- | :--- | :--- |
| 200 | இருநூறு | இருநூற்று- |
| 300 | முந்நூறு | முந்நூற்று- |
| 400 | நானூறு | நானூற்று- |
| 500 | ஐந்நூறு | ஐந்நூற்று- |
| 600 | அறுநூறு | அறுநூற்று- |
| 700 | எழுநூறு | எழுநூற்று- |
| 800 | எண்ணூறு | எண்ணூற்று- |
| 900 | தொள்ளாயிரம் | தொள்ளாயிரத்து- |

### 3.2 High Multipliers

| Value | Tamil Name | Grouping in Indian System |
| :--- | :--- | :--- |
| 1,000 | ஆயிரம் | `1,000` |
| 10,000 | பத்தாயிரம் | `10,000` |
| 1,00,000 (1 Lakh) | ஒரு லட்சம் / இலட்சம் | `1,00,000` |
| 10,00,000 (10 Lakhs) | பத்து லட்சம் / இலட்சம் | `10,00,000` |
| 1,00,00,000 (1 Crore) | ஒரு கோடி | `1,00,00,000` |

### 3.3 Scripture Example Parsing

1. **Revelation 7:4**:
   - Scripture Text: "முத்திரையிடப்பட்டவர்கள் ஒரு லட்சத்து நாற்பத்து நான்காயிரம்பேர்..."
   - Conversion: `ஒரு லட்சத்து நாற்பத்து நான்காயிரம்` $\rightarrow$ `1,44,000`
2. **Genesis 25:7**:
   - Abraham lived: "நூற்று எழுபத்தைந்து" (175)
   - Rule: 175 is in range 0–199. If written as `175`, it converts to `நூற்று எழுபத்தைந்து`. If already written as text, it remains text.
3. **2 Samuel 24:9**:
   - "இஸ்ரவேலிலே வாளுருகத்தக்க பலவான்கள் எட்டு லட்சம் பேரும்..."
   - Conversion: `எட்டு லட்சம்` $\rightarrow$ `8,00,000`

---

## 4. Indian Number Formatting Standard (`1,23,456,789`)

Numbers converted to digits are formatted according to the Indian numbering system:
- Last 3 digits: `xxx`
- Every 2 preceding digits: `xx,xx,xxx`
- Full example: `1,23,456,789` (One arab, twenty-three crores, forty-five lakhs, sixty-seven thousand, seven hundred and eighty-nine)

---

## 5. Exclusions & Disambiguation Rules

1. **Biblical Book Names with Leading Digits**:
   - `1 சாமுவேல்`, `2 சாமுவேல்`, `1 இராஜாக்கள்`, `2 இராஜாக்கள்`, `1 நாளாகமம்`, `2 நாளாகமம்`
   - `1 கொரிந்தியர்`, `2 கொரிந்தியர்`, `1 தெசலோனிக்கேயர்`, `2 தெசலோனிக்கேயர்`
   - `1 தீமோத்தேயு`, `2 தீமோத்தேயு`, `1 பேதுரு`, `2 பேதுரு`, `1 யோவான்`, `2 யோவான்`, `3 யோவான்`
   - **RULE**: Numbers immediately preceding canonical biblical book names are NOT converted unless the user specifically targets book titles.
2. **Verse & Chapter Boundaries**:
   - Numbers immediately following `\c` or `\v` are never touched.
3. **Footnote/Reference markers**:
   - Text enclosed within `\r ...`, `\f ... \f*`, `\x ... \x*`, `\fig ... \fig*` is strictly excluded.
