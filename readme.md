# MtgCardParser 

This document will describe all the mechanics of the card parser.

# Idea

The general idea of the machine is as follows:
- The machine is fed an MTG card
- The machine parses the card
- If successfull, it will dump the XML representation of the card into a file

# Steps

When fed a card, the machine will run it through several steps:

## Text transformer pipeline

The pipeline if constructed out of several text transformers, each one takes the card text as input and transforms it.

**Example:**

*Lowercase*

Target creature gets -3/-0 until end of turn. -> target creature gets -3/-0 until end of turn.

## Parse tree

The most complex part of the system.